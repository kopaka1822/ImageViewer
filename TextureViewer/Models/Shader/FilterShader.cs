using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;
using TextureViewer.Models.Filter;

namespace TextureViewer.Models.Shader
{
    public class FilterShader
    {
        private readonly Program program;
        private readonly int maxTextureBindings;

        public static readonly int LocalSize = 8;
        public static readonly int MinWorkGroupSize = 4;

        private readonly bool isSepa;

        public FilterShader(string shaderSource, bool isSepa)
        {
            this.isSepa = isSepa;
            this.maxTextureBindings = GL.GetInteger(GetPName.MaxTextureImageUnits);

            var shader = new glhelper.Shader(ShaderType.ComputeShader,
                GetShaderHeader() +
                "#line 1\n" +
                shaderSource);

            shader.Compile();

            program = new Program(new List<glhelper.Shader>{shader}, true);
        }

        public void Dispose()
        {
            program.Dispose();
        }

        public void Bind()
        {
            program.Bind();
        }

        public void Dispatch(int width, int height, List<IFilterParameter> parameters) =>
            Dispatch(width, height, parameters, 0);

        /// <summary>
        /// dispatching command for sepa shader
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="parameters">parameters to update</param>
        /// <param name="iteration">0 for first iteration, 1 for second iteration</param>
        public void Dispatch(int width, int height, List<IFilterParameter> parameters, int iteration)
        {
            program.Bind();
            // set parameter
            foreach (var parameter in parameters)
            {
                switch (parameter.GetParamterType())
                {
                    case ParameterType.Float:
                        GL.Uniform1(parameter.GetBase().Location, parameter.GetFloatModel().Value);
                        break;
                    case ParameterType.Int:
                        GL.Uniform1(parameter.GetBase().Location, parameter.GetIntModel().Value);
                        break;
                    case ParameterType.Bool:
                        GL.Uniform1(parameter.GetBase().Location, parameter.GetBoolModel().Value?1:0);
                        break;
                }
            }

            if (isSepa)
            {
                // set direction for sepa shader
                Debug.Assert(iteration == 1 || iteration == 0);
                GL.Uniform2(0, iteration, 1 - iteration);
            }
            else
            {
                Debug.Assert(iteration == 0);
            }

            GL.DispatchCompute(width / LocalSize + 1, height / LocalSize + 1, 1);
            Program.Unbind();
        }

        public int GetSourceImageLocation()
        {
            return 0;
        }

        /// <summary>
        /// returns the binding point for the original images (the imported images)
        /// </summary>
        /// <param name="index">original image index (starting with 0)</param>
        /// <returns>binding point for the original images or -1 if the image can not be nound due to bounding point maximum</returns>
        public int GetOriginalImageLocation(int index)
        {
            // up to context.MaxTextureBindings textures can be bound at once
            // one texture is used for the source image => context.MaxTextureBindings - 1 slots for the original images
            if (index > maxTextureBindings - 1)
                return -1;
            return index + 1;
        }

        public int GetDestinationImageLocation()
        {
            return 1;
        }

        private string GetShaderHeader()
        {
            return "#version 430\n" +
                   $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                   "layout(binding = 0) uniform sampler2D src_image;\n" +
                   GetTextureBindings(maxTextureBindings - 1) +
                   "layout(rgba32f, binding = 1) uniform writeonly image2D dst_image;\n" +
                   "layout(location = 1) uniform ivec2 pixelOffset;\n" +
                   (isSepa ? "layout(location = 0) uniform ivec2 filterDirection;\n" : "");
        }

        private string GetTextureBindings(int numBindings)
        {
            string res = "";
            for (int i = 0; i < numBindings; ++i)
                res += $"layout(binding = {i + 1}) uniform sampler2D texture{i};\n";
            return res;
        }
    }
}
