using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.ImageContext
{
    class ImageCombineShader
    {
        private ImageContext context;
        private Program program;
        private bool contentChanged = false;
        private const int LocalSize = 8;
        private const int TextureBindingStart = 3;

        public ImageCombineShader(ImageContext context)
        {
            this.context = context;
            context.ChangedImageCombineFormula += (sender, args) => contentChanged = true;
        }

        public void Update()
        {
            if (program == null || contentChanged)
            {
                program?.Dispose();

                // compile new shader
                var computeShader = new Shader(ShaderType.ComputeShader);
                computeShader.Source = GenerateShaderSource();
                computeShader.Compile();

                program = new Program(new List<Shader>{computeShader}, true);
            }
        }

        /// <summary>
        /// dispatches the shader with the given layer and level
        /// target image and source textures have to be bound before a call to this function
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="level"></param>
        /// <param name="width">mipmap width</param>
        /// <param name="height">mipmap height</param>
        public void Run(int layer, int level, int width, int height)
        {
            program.Bind();
            SetLevel(level);
            SetLayer(layer);
            GL.DispatchCompute(width / LocalSize + 1, height / LocalSize + 1, 1);
        }

        public int GetDestinationImageBinding()
        {
            return 0;
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

        private string GenerateShaderSource()
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
                   GetTextureBindings() +
                   GetTextureGetters() +
                   "void main(){\n" +
                   "pixelPos = ivec2(gl_GlobalInvocationID.xy);\n" +
                   "ivec2 imgSize = imageSize(out_image);\n" +
                   "if(pixelPos.x >= imgSize.x || pixelPos.y >= imgSize.y) return;\n" +
                   "vec4 color = " + GetImageColor() + ";\n" +
                   "imageStore(out_image, pixelPos, color);\n" +
                   "}\n";
        }

        private string GetTextureBindings()
        {
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
            {
                res += $"layout(binding = {i + TextureBindingStart}) uniform sampler2DArray texture{i};\n";
            }
            return res;
        }

        private string GetImageColor()
        {
            return context.ImageCombineFormula;
        }

        private string GetTextureGetters()
        {
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
            {
                res += $"vec4 GetTexture{i}(){'{'}\n" +
                       $"return texelFetch(texture{i}, ivec3(pixelPos, layer), level);\n" +
                       "}\n";
            }
            return res;
        }
    }
}
