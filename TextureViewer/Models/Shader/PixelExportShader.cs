using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;

namespace TextureViewer.Models.Shader
{
    public class PixelExportShader
    {
        private readonly Program shaderProgram;
        private static readonly int LocalSize = 32;

        public PixelExportShader()
        {
            shaderProgram = new Program(new List<glhelper.Shader>{new glhelper.Shader(ShaderType.ComputeShader, GetComputeSource()).Compile()}, true);
        }

        public void Dispose()
        {
            shaderProgram?.Dispose();
        }

        public int GetSourceTextureLocation()
        {
            return 0;
        }

        public int GetDestinationTextureLocation()
        {
            return 1;
        }

        /// <summary>
        /// copies the specified area from the source texture to the destination texture.
        /// Note that both texture must be bound before calling this function
        /// </summary>
        /// <param name="xOffset">source pixel offset</param>
        /// <param name="yOffset">source pixel offset</param>
        /// <param name="width">destination pixel width</param>
        /// <param name="height">destination pixel height</param>
        /// <param name="toSrgb">indicates if conversion into srgb space should be done</param>
        public void Use(int xOffset, int yOffset, int width, int height, bool toSrgb)
        {
            shaderProgram.Bind();
            GL.Uniform2(0, xOffset, yOffset);
            GL.Uniform1(1, toSrgb?1:0);
            GL.DispatchCompute(width / LocalSize + 1, height / LocalSize + 1, 1);
            Program.Unbind();
        }

        private static string GetComputeSource()
        {
            return OpenGlContext.ShaderVersion + "\n" +
                $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                SrgbShader.ToSrgbFunction() +
                @"layout(binding = 0) uniform sampler2D src;
                  layout(rgba32f, binding = 1) uniform writeonly image2D dst;
                  layout(location = 0) uniform ivec2 offset;
                  layout(location = 1) uniform bool srgb;                  
                  
                  void main(){
                    ivec2 coord = ivec2(gl_GlobalInvocationID.xy);
                    if(coord.x < imageSize(dst).x &&
                       coord.y < imageSize(dst).y) {
                        vec4 pixel = texelFetch(src, coord + offset, 0);
                        if(srgb) pixel = toSrgb(pixel);
                        imageStore(dst, coord, pixel);
                    }
                }";
        }
    }
}
