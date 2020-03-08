using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageFramework.Model.Progress
{
    interface IProgress : IProgress<float>
    {
        // progress between 0 and 1
        float Progress { get; set; }
        string What { set; }

        CancellationToken Token { get; }

        // creates a sub progress that updates this progress between the current Progress value and maxProgress
        IProgress CreateSubProgress(float maxProgress);
    }
}
