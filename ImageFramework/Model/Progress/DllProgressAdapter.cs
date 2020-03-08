using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;

namespace ImageFramework.Model.Progress
{
    internal class DllProgressAdapter : IDisposable
    {
        private readonly IProgress parent;

        public DllProgressAdapter(IProgress parent)
        {
            this.parent = parent;

            Dll.set_progress_callback(OnDllProgress);
        }

        private uint OnDllProgress(float progress, string description)
        {
            parent.Progress = progress;
            parent.What = description;

            return parent.Token.IsCancellationRequested ? (uint)1 : (uint)0;
        }

        public void Dispose()
        {
            Dll.set_progress_callback(null);
        }
    }
}
