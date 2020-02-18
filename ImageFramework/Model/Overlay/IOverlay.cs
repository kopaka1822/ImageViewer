using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageFramework.Model.Overlay
{
    public interface IOverlay : IDisposable
    {
        // draw overlay over the currently bound texture
        void Render(LayerMipmapSlice lm, Size3 size);

        event EventHandler HasChanged;

        // indicates that this overlay will actually render something
        bool HasWork { get; }
    }

    public abstract class OverlayBase : IOverlay
    {
        public abstract void Render(LayerMipmapSlice lm, Size3 size);

        public event EventHandler HasChanged;
        public virtual bool HasWork => true;

        protected virtual void OnHasChanged()
        {
            HasChanged?.Invoke(this, EventArgs.Empty);
        }

        public abstract void Dispose();
    }
}
