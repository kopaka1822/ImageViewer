using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.ImageCombination;
using TextureViewer.Models.Filter;
using TextureViewer.Utility;

namespace TextureViewer.Controller.Filter
{
    public class SingleDispatchStepper : FilterStepableBase, IStepable
    {
        private int currentStep = 0;
        private readonly int width;
        private readonly int height;

        public SingleDispatchStepper(Models.Models models, FilterModel model, ImageCombineBuilder builder, int layer, int iteration) :
            base(models, model, builder, layer: layer, iteration: iteration)
        {
            this.width = models.Images.Width;
            this.height = models.Images.Height;
        }

        public int GetNumSteps()
        {
            return 1;
        }

        public int CurrentStep()
        {
            return currentStep;
        }

        public void NextStep()
        {
            Debug.Assert(currentStep == 0);
            BindProgramAndUniforms();

            // pixel position (starts always at 0 0 in single invocation)
            GL.Uniform2(1, 0, 0);
            GL.DispatchCompute(width, height, 1);

            ++currentStep;
        }

        public bool HasStep()
        {
            return currentStep == 0;
        }

        public string GetDescription()
        {
            return Model.Filename;
        }
    }
}
