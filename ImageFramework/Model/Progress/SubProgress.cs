using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ImageFramework.Model.Progress
{
    /// <summary>
    /// progress class that reports its own sub-progress to its parent
    /// </summary>
    internal class SubProgress : IProgress
    {
        private readonly IProgress parent;
        private readonly float parentMin;
        private readonly float parentLength;

        public SubProgress(IProgress parent, float parentMax)
        {
            this.parent = parent;
            this.parentMin = parent.Progress;
            this.parentLength = parentMax - parent.Progress;
        }

        private float progress = 0.0f;
        public float Progress
        {
            get => progress;
            set
            {
                progress =  Utility.Utility.Clamp(value, 0.0f, 1.0f);
                // linear interpolate between those values
                parent.Progress = parentMin + progress * parentLength;
            }

        }

        // forward to parent
        public string What
        {
            set => parent.What = value;
        }

        public CancellationToken Token => parent.Token;

        public IProgress CreateSubProgress(float maxProgress)
        {
            maxProgress = Utility.Utility.Clamp(maxProgress, progress, 1.0f);
            return new SubProgress(this, maxProgress);
        }

        public void Report(float value)
        {
            Progress = value;
        }
    }
}
