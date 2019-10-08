using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace ImageViewer.Controller.TextureViews
{
    public class TextureViewData : IDisposable
    {
        public BlendState DefaultBlendState { get; }
        public BlendState AlphaBlendState { get; }

        public TextureViewData()
        {
            var dev = ImageFramework.DirectX.Device.Get();

            DefaultBlendState = CreateBlendState(false, BlendOption.One, BlendOption.Zero);
            AlphaBlendState = CreateBlendState(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);


        }

        private static BlendState CreateBlendState(bool enable, BlendOption src, BlendOption dst)
        {
            var dev = ImageFramework.DirectX.Device.Get();
            var blendDesc = new BlendStateDescription
            {
                AlphaToCoverageEnable = false,
                IndependentBlendEnable = false
            };
            blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
            {
                IsBlendEnabled = enable,
                BlendOperation = BlendOperation.Add,
                AlphaBlendOperation = BlendOperation.Add,
                SourceBlend = src,
                DestinationBlend = dst,
                SourceAlphaBlend = BlendOption.One,
                DestinationAlphaBlend = BlendOption.Zero,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };

            return new BlendState(dev.Handle, blendDesc);
        }

        public void Dispose()
        {
            DefaultBlendState?.Dispose();
            AlphaBlendState?.Dispose();
        }
    }
}
