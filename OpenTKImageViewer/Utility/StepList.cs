using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Utility
{
    class StepList : IStepable
    {
        public List<IStepable> Steps { get; } = new List<IStepable>();
        private int curStep = 0;
        private int numExecuted = 0;

        public bool HasStep()
        {
            return curStep < Steps.Count;
        }

        public void NextStep()
        {
            if (Steps[curStep].HasStep())
            {
                Steps[curStep].NextStep();
                numExecuted++;
                if (!Steps[curStep].HasStep())
                    ++curStep;
            }
        }

        public int CurrentStep()
        {
            return numExecuted;
        }

        public int GetNumSteps()
        {
            return Steps.Sum(stepable => stepable.GetNumSteps());
        }
    }
}
