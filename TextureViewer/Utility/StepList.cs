using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Utility
{
    /// <summary>
    /// multiple IStepables in one class
    /// </summary>
    class StepList : IStepable
    {
        private readonly List<IStepable> steps;
        // item index in list
        private int curItem = 0;

        public StepList(List<IStepable> steps)
        {
            this.steps = steps;
        }


        public int GetNumSteps()
        {
            return steps.Sum(s => s.GetNumSteps());
        }

        public int CurrentStep()
        {
            Debug.Assert(curItem < steps.Count);
            return steps.GetRange(0, curItem).Sum(s => s.GetNumSteps()) + steps[curItem].CurrentStep();
        }

        public void NextStep()
        {
            Debug.Assert(curItem < steps.Count);
            // advance current item
            if (steps[curItem].HasStep())
            {
                steps[curItem].NextStep();
            }

            // advance to next item
            if (!steps[curItem].HasStep())
                ++curItem;
        }

        public bool HasStep()
        {
            return curItem < steps.Count;
        }

        public string GetDescription()
        {
            Debug.Assert(curItem < steps.Count);
            return steps[curItem].GetDescription();
        }
    }
}
