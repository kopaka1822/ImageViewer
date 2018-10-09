using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.ImageCombination;
using TextureViewer.Models.Filter;
using TextureViewer.Models.Shader;
using TextureViewer.Utility;

namespace TextureViewer.Controller.Filter
{
    public class MultiDispatchStepper : FilterStepableBase, IStepable
    {
        // number of invocations in x
        private readonly int width;
        // number of invocations in y
        private readonly int height;
        // invocation offset x
        private int curX = 0;
        // invocation offset y
        private int curY = 0;

        public MultiDispatchStepper(Models.Models models, FilterModel model, ImageCombineBuilder builder, int layer, int iteration) : 
            base(models, model, builder, layer: layer, iteration: iteration)
        {
            this.width = GetNumMinimalInvocations(models.Images.Width);
            this.height = GetNumMinimalInvocations(models.Images.Height);
        }

        public int GetNumSteps()
        {
            return width * height;
        }

        public int CurrentStep()
        {
            return curY * width + curX;
        }

        public void NextStep()
        {
            BindProgramAndUniforms();

            // pixel position
            GL.Uniform2(1, curX * (FilterShader.LocalSize * FilterShader.MinWorkGroupSize), curY * (FilterShader.LocalSize * FilterShader.MinWorkGroupSize));
            GL.DispatchCompute(FilterShader.MinWorkGroupSize, FilterShader.MinWorkGroupSize, 1);

            if (++curX < width)
                return;

            curX = 0;
            ++curY;
        }

        public bool HasStep()
        {
            return curY < height;
        }

        public string GetDescription()
        {
            return Model.Filename;
        }
    }
}
