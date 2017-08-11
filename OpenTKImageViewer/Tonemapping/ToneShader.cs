using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.Tonemapping
{
    public class ToneShader
    {
        private Program program;
        private ShaderLoader loader;

        private const int LocalSize = 8;

        public ToneShader(ShaderLoader loader)
        {
            this.loader = loader;
            // generate missing source
            Shader shader = new Shader(ShaderType.ComputeShader,
                GetShaderHeader(loader.IsSepa) +
                            "#line 0\n" +
                            loader.ShaderSource);

            shader.Compile();

            program = new Program(new List<Shader> {shader}, true);
        }

        public int GetSourceImageLocation()
        {
            return 0;
        }

        public int GetDestinationImageLocation()
        {
            return 1;
        }

        public void Dispatch(int width, int height) => Dispatch(width, height, 0);

        /// <summary>
        /// dispatching command for sepa shader
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="iteration">0 for first iteration, 1 for second iteration</param>
        public void Dispatch(int width, int height, int iteration)
        {
            program.Bind();
            // set parameter
            foreach (var parameter in loader.Parameters)
            {
                switch (parameter.Type)
                {
                    case ShaderLoader.ParameterType.Float:
                        GL.Uniform1(parameter.Location, (float)parameter.CurrentValue);
                        break;
                    case ShaderLoader.ParameterType.Int:
                    case ShaderLoader.ParameterType.Bool:
                        GL.Uniform1(parameter.Location, (int)parameter.CurrentValue);
                        break;
                }
            }

            if (loader.IsSepa)
            {
                // set direction for sepa shader
                Debug.Assert(iteration == 1 || iteration == 0);
                GL.Uniform2(0, iteration, 1 - iteration);
            }

            GL.DispatchCompute(width / LocalSize + 1, height / LocalSize + 1, 1);
        }

        private string GetShaderHeader(bool isSepa)
        {
            return "#version 430\n" +
                   $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                   "layout(rgba32f, binding = 0) uniform readonly image2D src_image;\n" +
                   "layout(rgba32f, binding = 1) uniform readonly image2D dst_image;\n" +
                   (isSepa?"layout(location = 0) uniform ivec2 dir;\n":"");

        }
    }
}
