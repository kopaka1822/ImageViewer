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
        
        // draw overlay by utilizing d2d
        void RenderD2D(LayerMipmapSlice lm, Size3 size, DirectX.Direct2D d2d);

        event EventHandler HasChanged;

        // indicates that this overlay will actually render something
        bool HasWork { get; }

        // indicates that this overlay requires the d2d overlay which will result in RenderD2D being called
        bool RequireD2D { get; }
    }

    public abstract class OverlayBase : IOverlay
    {
        public abstract void Render(LayerMipmapSlice lm, Size3 size);

        public virtual void RenderD2D(LayerMipmapSlice lm, Size3 size, DirectX.Direct2D d2d)
        {
            
        }

        public event EventHandler HasChanged;
        public virtual bool HasWork => true;

        public virtual bool RequireD2D => false;

        protected virtual void OnHasChanged()
        {
            HasChanged?.Invoke(this, EventArgs.Empty);
        }

        public abstract void Dispose();
    }
}
