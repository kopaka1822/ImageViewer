using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Utility
{
    /// <summary>
    /// work that was split into several steps
    /// </summary>
    public interface IStepable
    {
        // total amount of steps
        int GetNumSteps();
        // step that is currently executed
        int CurrentStep();
        // executes the next step
        void NextStep();
        // may NextStep() be called at least one more time?
        bool HasStep();
        // description of the current step
        string GetDescription();
    }
}
