using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.Utility;

namespace OpenTKImageViewer.Tonemapping
{
    public class ToneShader
    {
        private readonly Program program;

        private static readonly int LocalSize = 32;

        public string Name { get; }
        public string Description { get; }
        public bool IsSepa { get; }

        public ToneShader(ShaderLoader loader)
        {
            this.IsSepa = loader.IsSepa;
            this.Name = loader.Name;
            this.Description = loader.Description;

            // generate missing source
            Shader shader = new Shader(ShaderType.ComputeShader,
                GetShaderHeader() +
                            "#line 1\n" +
                            loader.ShaderSource);

            shader.Compile();

            program = new Program(new List<Shader> {shader}, true);
        }

        public void Dispose()
        {
            program.Dispose();
        }

        public int GetSourceImageLocation()
        {
            return 0;
        }

        public int GetDestinationImageLocation()
        {
            return 1;
        }

        public void Dispatch(int width, int height, List<ShaderLoader.Parameter> parameters) => Dispatch(width, height, parameters, 0);

        /// <summary>
        /// dispatching command for sepa shader
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="parameters">parameters to update</param>
        /// <param name="iteration">0 for first iteration, 1 for second iteration</param>
        public void Dispatch(int width, int height, List<ShaderLoader.Parameter> parameters, int iteration)
        {
            program.Bind();
            // set parameter
            foreach (var parameter in parameters)
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

            if (IsSepa)
            {
                // set direction for sepa shader
                Debug.Assert(iteration == 1 || iteration == 0);
                GL.Uniform2(0, iteration, 1 - iteration);
            }

            GL.DispatchCompute(width / LocalSize + 1, height / LocalSize + 1, 1);
        }

        private class DispatchStepper : IStepable
        {
            private readonly Program program;
            private readonly List<ShaderLoader.Parameter> parameters;
            private readonly bool isSepa;
            private readonly int iteration;
            private int width;
            private int height;
            private int curX = 0;
            private int curY = 0;

            public DispatchStepper(Program program, List<ShaderLoader.Parameter> parameters, bool isSepa, int iteration, int imgWidth, int imgHeight)
            {
                this.program = program;
                this.parameters = parameters;
                this.isSepa = isSepa;
                this.iteration = iteration;
                width = imgWidth / LocalSize + (imgWidth % LocalSize !=0 ? 1 : 0);
                height = imgHeight / LocalSize + (imgHeight % LocalSize != 0 ? 1 : 0);
            }

            public bool HasStep()
            {
                return curY < height;
            }

            public void NextStep()
            {
                program.Bind();
                // set parameter
                foreach (var parameter in parameters)
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

                if (isSepa)
                {
                    // set direction for sepa shader
                    Debug.Assert(iteration == 1 || iteration == 0);
                    GL.Uniform2(0, iteration, 1 - iteration);
                }

                // pixel position
                GL.Uniform2(1, curX * LocalSize, curY * LocalSize);
                GL.DispatchCompute(1, 1, 1);

                if (++curX < width)
                    return;

                curX = 0;
                ++curY;
            }

            public float CurrentStep()
            {
                return (float)(curY * width + curX) / GetNumSteps();
            }

            public int GetNumSteps()
            {
                return width * height;
            }

            public string GetDescription()
            {
                return "";
            }
        }

        public IStepable GetDispatchStepable(int width, int height, List<ShaderLoader.Parameter> parameters, int iteration)
        {
            return new DispatchStepper(program, parameters, IsSepa, iteration, width, height);
        }

        private string GetShaderHeader()
        {
            return "#version 430\n" +
                   $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                   "layout(rgba32f, binding = 0) uniform readonly image2D src_image;\n" +
                   "layout(rgba32f, binding = 1) uniform writeonly image2D dst_image;\n" +
                   "layout(location = 1) uniform ivec2 pixelOffset;\n" +
                   (IsSepa?"layout(location = 0) uniform ivec2 filterDirection;\n":"");

        }
    }
}
