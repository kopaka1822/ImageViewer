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
        private readonly int maxTextureBindings;

        private static readonly int LocalSize = 8;
        private static readonly int MinWorkGroupSize = 4;

        public string Name { get; }
        public string Description { get; }
        public bool IsSepa { get; }
        public bool IsSingleInvocation { get; }

        public ToneShader(ShaderLoader loader)
        {
            this.IsSepa = loader.IsSepa;
            this.Name = loader.Name;
            this.Description = loader.Description;
            this.IsSingleInvocation = loader.IsSingleInvocation;
            this.maxTextureBindings = GL.GetInteger(GetPName.MaxTextureImageUnits);

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

        /// <summary>
        /// returns the binding point for the original images (the imported images)
        /// </summary>
        /// <param name="index">original image index (starting with 0)</param>
        /// <param name="context"></param>
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

        public void Dispatch(int width, int height, List<ShaderLoader.Parameter> parameters) =>
            Dispatch(width, height, parameters, 0);

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
                        GL.Uniform1(parameter.Location, (float) parameter.CurrentValue);
                        break;
                    case ShaderLoader.ParameterType.Int:
                    case ShaderLoader.ParameterType.Bool:
                        GL.Uniform1(parameter.Location, (int) parameter.CurrentValue);
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
            Program.Unbind();
        }

        private class DispatchStepper
        {
            private readonly Program program;
            private readonly List<ShaderLoader.Parameter> parameters;
            private readonly bool isSepa;
            private readonly int iteration;

            protected DispatchStepper(Program program, List<ShaderLoader.Parameter> parameters, bool isSepa,
                int iteration)
            {
                this.program = program;
                this.parameters = parameters;
                this.isSepa = isSepa;
                this.iteration = iteration;
            }

            protected void BindProgramAndUniforms()
            {
                program.Bind();
                // set parameter
                foreach (var parameter in parameters)
                {
                    switch (parameter.Type)
                    {
                        case ShaderLoader.ParameterType.Float:
                            GL.Uniform1(parameter.Location, (float) parameter.CurrentValue);
                            break;
                        case ShaderLoader.ParameterType.Int:
                        case ShaderLoader.ParameterType.Bool:
                            GL.Uniform1(parameter.Location, (int) parameter.CurrentValue);
                            break;
                    }
                }

                if (isSepa)
                {
                    // set direction for sepa shader
                    Debug.Assert(iteration == 1 || iteration == 0);
                    GL.Uniform2(0, iteration, 1 - iteration);
                }
            }

            /// <summary>
            /// converts amount of pixels into number of work groups (division through local size)
            /// </summary>
            /// <param name="pixels"></param>
            /// <returns></returns>
            protected static int GetNumWorkGroups(int pixels)
            {
                return pixels / LocalSize + (pixels % LocalSize != 0 ? 1 : 0);
            }

            /// <summary>
            /// converts amount of pixels into minimal number of shader invocations 
            /// (depending on LocalSize and MinWorkGroupSize)
            /// </summary>
            /// <param name="pixels"></param>
            /// <returns></returns>
            protected static int GetNumMinimalInvocations(int pixels)
            {
                return pixels / (LocalSize * MinWorkGroupSize) + (pixels % (LocalSize * MinWorkGroupSize) != 0 ? 1 : 0);
            }
        }

        /// <summary>
        /// Stepper for dispatching the shader if only single invocations are used
        /// </summary>
        private class SingleDispatchStepper : DispatchStepper, IStepable
        {
            private int width;
            private int height;
            private int curStep = 0;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="program">corresponding shader</param>
            /// <param name="parameters">parameters</param>
            /// <param name="isSepa">is shader seperatable</param>
            /// <param name="iteration">which sepa iteration</param>
            /// <param name="imgWidth">width of the image</param>
            /// <param name="imgHeight">height of the image</param>
            public SingleDispatchStepper(Program program, List<ShaderLoader.Parameter> parameters, bool isSepa,
                int iteration, int imgWidth, int imgHeight)
                : base(program, parameters, isSepa, iteration)
            {
                width = GetNumWorkGroups(imgWidth);
                height = GetNumWorkGroups(imgHeight);
            }

            public float CurrentStep()
            {
                // there is only one step (0 = 0%, 1 = 1%)
                return (float) curStep;
            }

            public string GetDescription()
            {
                return "";
            }

            public int GetNumSteps()
            {
                return 1;
            }

            public bool HasStep()
            {
                return curStep == 0;
            }

            public void NextStep()
            {
                Debug.Assert(curStep == 0);
                BindProgramAndUniforms();

                // pixel position (starts always at 0 0 in single invocation)
                GL.Uniform2(1, 0, 0);
                GL.DispatchCompute(width, height, 1);

                curStep++;
            }
        }

        /// <summary>
        /// Stepper for Dispatching if the shader is run in multiple invocations
        /// </summary>
        private class MultiDispatchStepper : DispatchStepper, IStepable
        {
            private int width;
            private int height;
            private int curX = 0;
            private int curY = 0;

            public MultiDispatchStepper(Program program, List<ShaderLoader.Parameter> parameters, bool isSepa,
                int iteration, int imgWidth, int imgHeight)
                : base(program, parameters, isSepa, iteration)
            {
                width = GetNumMinimalInvocations(imgWidth);
                height = GetNumMinimalInvocations(imgHeight);
            }

            public bool HasStep()
            {
                return curY < height;
            }

            public void NextStep()
            {
                BindProgramAndUniforms();

                // pixel position
                GL.Uniform2(1, curX * (LocalSize * MinWorkGroupSize), curY * (LocalSize * MinWorkGroupSize));
                GL.DispatchCompute(MinWorkGroupSize, MinWorkGroupSize, 1);

                if (++curX < width)
                    return;

                curX = 0;
                ++curY;
            }

            public float CurrentStep()
            {
                return (float) (curY * width + curX) / GetNumSteps();
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

        public IStepable GetDispatchStepable(int width, int height, List<ShaderLoader.Parameter> parameters,
            int iteration)
        {
            if (IsSingleInvocation)
                return new SingleDispatchStepper(program, parameters, IsSepa, iteration, width, height);
            return new MultiDispatchStepper(program, parameters, IsSepa, iteration, width, height);
        }

        private string GetShaderHeader()
        {
            return "#version 430\n" +
                   $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                   "layout(binding = 0) uniform sampler2D src_image;\n" +
                   GetTextureBindings(maxTextureBindings - 1) +
                   "layout(rgba32f, binding = 1) uniform writeonly image2D dst_image;\n" +
                   "layout(location = 1) uniform ivec2 pixelOffset;\n" +
                   (IsSepa ? "layout(location = 0) uniform ivec2 filterDirection;\n" : "");
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