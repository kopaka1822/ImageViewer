using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Models;
using TextureViewer.Utility;

namespace TextureViewer.Controller.ImageCombination
{
    /// <summary>
    /// applies the current state of the image combine builder to the final image model
    /// </summary>
    public class FinalImageStepable : IStepable
    {
        private int curStep = 0;
        private readonly ImageCombineBuilder builder;
        private readonly FinalImageModel finalImage;

        public FinalImageStepable(ImageCombineBuilder builder, FinalImageModel finalImage)
        {
            this.builder = builder;
            this.finalImage = finalImage;
        }

        public int GetNumSteps()
        {
            return 1;
        }

        public int CurrentStep()
        {
            return curStep;
        }

        public void NextStep()
        {
            Debug.Assert(curStep == 0);

            finalImage.Apply(builder.GetPrimaryTexture(), builder.GetStatisticsTexture());

            builder.Dispose();

            ++curStep;
        }

        public bool HasStep()
        {
            return curStep == 0;
        }

        public string GetDescription()
        {
            return "applying changes";
        }
    }
}
