using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Utility;

namespace TextureViewer.Controller.ImageCombination
{
    public class StatisticsSaveStepable : IStepable
    {
        private int curStep = 0;
        private readonly ImageCombineBuilder builder;

        public StatisticsSaveStepable(ImageCombineBuilder builder)
        {
            this.builder = builder;
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
            builder.UsePrimaryAsStatistics();

            ++curStep;
        }

        public bool HasStep()
        {
            return curStep == 0;
        }

        public string GetDescription()
        {
            return "saving statistics image";
        }
    }
}
