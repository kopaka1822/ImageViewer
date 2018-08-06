using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;
using TextureViewer.Utility;

namespace TextureViewer.Models.Shader.Statistics
{
    public abstract class StatisticsShader
    {
        private Program program;

        public static readonly int LocalSize = 1;

        protected StatisticsShader()
        {}

        protected void init()
        {
            var shader = new glhelper.Shader(ShaderType.ComputeShader, GetComputeSource()).Compile();
            program = new Program(new List<glhelper.Shader> { shader }, true);
        }

        public void Dispose()
        {
            program.Dispose();
        }

        private int DivideRoundUp(int a, int b)
        {
            return (a + b - 1) / b;
        }

        /// <summary>
        /// executes the shader for the outer image layer
        /// </summary>
        /// <returns></returns>
        public Color Run(TextureArray2D source, Models models)
        {
            var texDst = models.GlData.TextureCache.GetTexture();

            models.GlData.BindSampler(0, false, false);
            source.BindAsTexture2D(0, 0, 0);
            texDst.BindAsImage(1, 0, 0, TextureAccess.WriteOnly);

            // bind and set uniforms
            program.Bind();
            // direction
            GL.Uniform2(0, 1, 0);
            // stride
            var curStride = 2;
            GL.Uniform1(1, curStride);

            var curWidth = models.Images.Width;
            GL.DispatchCompute(DivideRoundUp(curWidth, LocalSize * 2), models.Images.Height, 1);

            // swap textures
            var texSrc = models.GlData.TextureCache.GetTexture();
            GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);

            // do invocation until finished
            while (curWidth > 2)
            {
                //curWidth /= 2;
                curWidth = DivideRoundUp(curWidth, 2);
                curStride *= 2;

                // swap textures
                Swap(ref texSrc, ref texDst);
                texSrc.BindAsTexture2D(0, 0, 0);
                texDst.BindAsImage(1, 0, 0, TextureAccess.WriteOnly);

                // stride
                GL.Uniform1(1, curStride);

                // dispatch
                GL.DispatchCompute(DivideRoundUp(curWidth, LocalSize * 2), models.Images.Height, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);
            }

            // do the scan in y direction
            var curHeight = models.Images.Height;
            curStride = 2;

            // set direction
            GL.Uniform2(0, 0, 1);

            while (curHeight > 1)
            {
                // swap textures
                Swap(ref texSrc, ref texDst);
                texSrc.BindAsTexture2D(0, 0, 0);
                texDst.BindAsImage(1, 0, 0, TextureAccess.WriteOnly);

                // stride
                GL.Uniform1(1, curStride);

                GL.DispatchCompute(1, DivideRoundUp(curHeight, LocalSize * 2), 1);
                GL.MemoryBarrier(MemoryBarrierFlags.TextureFetchBarrierBit);

                curHeight = DivideRoundUp(curHeight, 2);
                curStride *= 2;
            }

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            // the result is in pixel 0 0

            texDst.BindAsTexture2D(models.GlData.GetPixelShader.GetTextureLocation(), 0, 0);
            // y coordinates for the texture fetch are reverted
            var res = models.GlData.GetPixelShader.GetPixelColor(0, models.Images.Height - 1, 0);

            // cleanup
            models.GlData.TextureCache.StoreTexture(texSrc);
            models.GlData.TextureCache.StoreTexture(texDst);

            return ModifyResult(res);
        }

        private void Swap(ref TextureArray2D t1, ref TextureArray2D t2)
        {
            var tmp = t1;
            t1 = t2;
            t2 = tmp;
        }

        private string GetComputeSource()
        {
            return OpenGlContext.ShaderVersion + "\n" +
                   $"layout(local_size_x = {LocalSize}) in;\n" +
                   $"const int LOCAL_SIZE = {LocalSize};\n" +
                   @"layout(binding = 0) uniform sampler2D src_image;
                   layout(binding = 1) uniform writeonly image2D dst_image;
                   layout(location = 0) uniform ivec2 direction;
                   layout(location = 1) uniform int stride;" +
                   GetFunctions() +
                   "\nvec4 combine(vec4 a, vec4 b) {" + 
                   GetCombineFunction() + 
                   "}\n" +
                   "vec4 combineSingle(vec4 a) {\n" +
                   GetSingleCombine() +
                   "}\n" +
                   @"#line 1 10
                    int idot(ivec2 a, ivec2 b) { return a.x * b.x + a.y * b.y; }

                    void main() {
                        const ivec2 invDir = ivec2(1) - direction;

                        ivec2 pixelX = (idot(ivec2(gl_WorkGroupID), direction) * LOCAL_SIZE + int(gl_LocalInvocationID)) * direction;
                        ivec2 pixelY = idot(ivec2(gl_WorkGroupID), invDir) * invDir;
                        ivec2 pixel = pixelX + pixelY;

                        ivec2 y = idot(pixel, invDir) * invDir;
                        ivec2 x = idot(pixel, direction) * stride * direction;
                        ivec2 x2 = x + direction * (stride / 2);
                        
                        ivec2 pos1 = x + y;
                        ivec2 pos2 = x2 + y;

                        ivec2 size = ivec2(textureSize(src_image, 0));
                        if(pos1.x >= size.x || pos1.y >= size.y) return;
                        if(pos2.x >= size.x || pos2.y >= size.y)
                        {
                            /* only write the value as is */
                            vec4 color = combineSingle(texelFetch(src_image, pos1, 0));
                            if(any(isnan(color))) color = vec4(0);
                            imageStore(dst_image, pos1, color);
                            return;
                        }
                        vec4 color = combine( texelFetch(src_image, pos1, 0), texelFetch(src_image, pos2, 0) );
                        if(any(isnan(color))) color = vec4(0);
                        //imageStore(dst_image, pos1, vec4(float(pos2.x)));
                        imageStore(dst_image, pos1, color);

                    }";
        }

        /// <summary>
        /// function which combines vec4 a and vec4 b
        /// </summary>
        /// <returns></returns>
        protected abstract string GetCombineFunction();

        /// <summary>
        /// function that will be called if no partner for vec4 a exists (only vec4 a as argument)
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSingleCombine()
        {
            return "return a;";
        }

        protected virtual string GetFunctions()
        {
            return "";
        }

        protected virtual Color ModifyResult(Color color)
        {
            return color;
        }
    }
}
