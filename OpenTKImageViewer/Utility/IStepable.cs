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
        int CurrentStep();
        int GetNumSteps();
    }
}
