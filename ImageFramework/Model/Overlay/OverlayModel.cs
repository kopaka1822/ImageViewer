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

        private BlendState blendState;

        internal OverlayModel(ImageModelTextureCache cache)
        {
            this.cache = cache;
            cache.Changed += CacheOnChanged;
            Overlays.CollectionChanged += OverlaysOnCollectionChanged;

            var blendDesc = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            blendDesc.RenderTarget[0].IsBlendEnabled = true;
            blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            // C' = a_src * c_src + (1.0 - a_src) * a_dst
            blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;

            // A' = (1.0 - a_src) * a_dst (inverse alpha, render target starts with a == 1)
            blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
            blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;

            blendState = new BlendState(Device.Get().Handle, blendDesc);
        }

        private void OverlaysOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // invalidate images
            InvalidateOld();

            // subscribe to change events
            if(e.NewItems != null)
            foreach (var newItem in e.NewItems)
            {
                var o = (IOverlay) newItem;
                o.HasChanged += ListItemHasChanged;
            }

            // unsubscribe from change events
            if(e.OldItems != null)
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
        /// <summary>
        /// RGB channels = alpha premultiplied color
        /// A = occlusion (inverse alpha)
        /// </summary>
        public ITexture Overlay 
        { 
            get
            {
                if (overlayTex != null) return overlayTex;
                if (!cache.IsValid || !HasWork()) return null;

                // recompute texture
                var dev = Device.Get();
                overlayTex = cache.GetTexture();
                foreach (var lm in overlayTex.LayerMipmap.Range)
                {
                    // bind and clear rendertarget
                    dev.ClearRenderTargetView(overlayTex.GetRtView(lm), new RawColor4(0.0f, 0.0f, 0.0f, 1.0f));
                    dev.OutputMerger.SetRenderTargets(overlayTex.GetRtView(lm));
                    dev.OutputMerger.SetBlendState(blendState);
                    var size = overlayTex.Size.GetMip(lm.Mipmap);
                    dev.SetViewScissors(size.Width, size.Height);

                    // draw all overlays
                    foreach (var overlay in Overlays)
                    {
                        if(overlay.HasWork)
                            overlay.Render(lm, size);
                    }
                }
                
                // unbind rendertargets
                dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
                dev.OutputMerger.SetBlendState(null);

                return overlayTex;
            }
        }

        private bool HasWork()
        {
            if (Overlays.Count == 0) return false;
            return Overlays.Any(ol => ol.HasWork);
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
            blendState?.Dispose();
            foreach (var overlay in Overlays)
            {
                overlay.Dispose();
            }
        }

        public void Recompute()
        {
            var dummy = Overlay;
        }
    }
}
