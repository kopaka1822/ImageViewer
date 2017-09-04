using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Utility
{
    public interface IStepable
    {
        bool HasStep();
        void NextStep();
        float CurrentStep();
        int GetNumSteps();
        string GetDescription();
    }
}
