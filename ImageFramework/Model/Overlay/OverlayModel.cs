using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Overlay
{
    public class OverlayModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ImageModelTextureCache cache;
        public ObservableCollection<IOverlay> Overlays { get; } = new ObservableCollection<IOverlay>();

        internal OverlayModel(ImageModelTextureCache cache)
        {
            this.cache = cache;
            cache.Changed += CacheOnChanged;
            Overlays.CollectionChanged += OverlaysOnCollectionChanged;
        }

        private void OverlaysOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // invalidate images
            InvalidateOld();

            // subscribe to change events
            foreach (var newItem in e.NewItems)
            {
                var o = (IOverlay) newItem;
                o.HasChanged += ListItemHasChanged;
            }

            // unsubscribe from change events
            foreach (var oldItem in e.OldItems)
            {
                var o = (IOverlay)oldItem;
                o.HasChanged -= ListItemHasChanged;
                o.Dispose();
            }

            // this has definitively changed
            OnPropertyChanged(nameof(Overlay));
        }

        private void ListItemHasChanged(object sender, EventArgs e)
        {
            InvalidateOld();
            OnPropertyChanged(nameof(Overlay));
        }

        private void CacheOnChanged(object sender, EventArgs e)
        {
            // invalidate images
            if (overlayTex != null)
            {
                cache.StoreTexture(overlayTex);
                overlayTex = null;
                OnPropertyChanged(nameof(Overlay));
            }
        }

        private void InvalidateOld()
        {
            if (overlayTex != null)
            {
                cache.StoreTexture(overlayTex);
                overlayTex = null;
            }
        }

        private ITexture overlayTex;
        public ITexture Overlay 
        { 
            get
            {
                if (overlayTex != null) return overlayTex;
                if (!cache.IsValid || Overlays.Count == 0) return null;

                // recompute texture
                var dev = Device.Get();
                overlayTex = cache.GetTexture();
                for (int layer = 0; layer < overlayTex.NumLayers; ++layer)
                {
                    for (int mipmap = 0; mipmap < overlayTex.NumMipmaps; ++mipmap)
                    {
                        // bind and clear rendertarget
                        dev.ClearRenderTargetView(overlayTex.GetRtView(layer, mipmap), new RawColor4(0.0f, 0.0f, 0.0f, 0.0f));
                        dev.OutputMerger.SetRenderTargets(overlayTex.GetRtView(layer, mipmap));
                        var size = overlayTex.Size.GetMip(mipmap);

                        // draw all overlays
                        foreach (var overlay in Overlays)
                        {
                            overlay.Render(layer, mipmap, size);
                        }
                    }
                }

                // unbind rendertargets
                dev.OutputMerger.SetRenderTargets((RenderTargetView)null);

                return overlayTex;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            overlayTex?.Dispose();
            foreach (var overlay in Overlays)
            {
                overlay.Dispose();
            }
        }
    }
}
