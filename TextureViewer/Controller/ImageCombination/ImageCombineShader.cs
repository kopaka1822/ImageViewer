using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.ImageCombination
{
    class ImageCombineShader
    {
        private const int LocalSize = 8;
        private const int TextureBindingStart = 3;

        private readonly Program shader;

        public ImageCombineShader(string colorFormula, string alphaFormula, int numImages)
        {
            var computeShader = new Shader(ShaderType.ComputeShader)
            {
                Source = GenerateShaderSource(colorFormula, alphaFormula, Math.Max(numImages, 1))
            };

            computeShader.Compile();

            shader = new Program(new List<Shader>{computeShader}, true);
        }

        /// <summary>
        /// dispatches the shader with the given layer and level
        /// source textures have to be bound before a call to this function
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="level"></param>
        /// <param name="width">mipmap width</param>
        /// <param name="height">mipmap height</param>
        /// <param name="target">target image</param>
        public void Run(int layer, int level, int width, int height, TextureArray2D target)
        {
            shader.Bind();
            target.BindAsImage(GetDestinationImageBinding(), level, layer, TextureAccess.WriteOnly);
            SetLevel(level);
            SetLayer(layer);
            GL.DispatchCompute(width / LocalSize + 1, height / LocalSize + 1, 1);
            Program.Unbind();
        }

        public void Dispose()
        {
            shader.Dispose();
        }

        /// <summary>
        /// required binding for the source textures (starting with texture 0)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetSourceImageBinding(int id)
        {
            return TextureBindingStart + id;
        }

        private void SetLayer(int layer)
        {
            GL.Uniform1(1, layer);
        }

        private void SetLevel(int level)
        {
            GL.Uniform1(2, level);
        }

        private int GetDestinationImageBinding()
        {
            return 0;
        }

        #region STATIC HELPER

        private static string GenerateShaderSource(string colorFormula, string alphaFormula, int numImages)
        {
            return "#version 430 core\n" +
                   $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                   // output image
                   "layout(rgba32f, binding = 0) uniform writeonly image2D out_image;\n" +
                   // global variables
                   "ivec2 pixelPos;\n" +
                   // uniforms
                   "layout(location = 1) uniform int layer;\n" +
                   "layout(location = 2) uniform int level;\n" +
                   GetTextureBindings(numImages) +
                   GetTextureGetters(numImages) +
                   "void main(){\n" +
                   "pixelPos = ivec2(gl_GlobalInvocationID.xy);\n" +
                   "ivec2 imgSize = imageSize(out_image);\n" +
                   "if(pixelPos.x >= imgSize.x || pixelPos.y >= imgSize.y) return;\n" +
                   "vec4 color = " + GetImageColor(colorFormula, alphaFormula) + ";\n" +
                   "imageStore(out_image, pixelPos, color);\n" +
                   "}\n";
        }

        private static string GetTextureBindings(int numImages)
        {
            string res = "";
            for (int i = 0; i < numImages; ++i)
            {
                res += $"layout(binding = {i + TextureBindingStart}) uniform sampler2DArray texture{i};\n";
            }
            return res;
        }

        private static string GetImageColor(string alphaFormula, string colorFormula)
        {
            return $"vec4(({colorFormula}).rgb, ({alphaFormula}).a)";
        }

        private static string GetTextureGetters(int numImages)
        {
            string res = "";
            for (int i = 0; i < numImages; ++i)
            {
                res += $"vec4 GetTexture{i}(){'{'}\n" +
                       $"return texelFetch(texture{i}, ivec3(pixelPos, layer), level);\n" +
                       "}\n";
            }
            return res;
        }

        #endregion
    }
}
