using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace ImageViewer.Controller.TextureViews
{
    public class TextureViewData : IDisposable
    {
        public BlendState DefaultBlendState { get; }
        public BlendState AlphaBlendState { get; }

        public SamplerState LinearSampler { get; }

        public SamplerState PointSampler { get; }
        public TextureViewData()
        {
            var dev = ImageFramework.DirectX.Device.Get();

            DefaultBlendState = CreateBlendState(false, BlendOption.One, BlendOption.Zero);
            AlphaBlendState = CreateBlendState(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);

            LinearSampler = CreateSamplerState(true);
            PointSampler = CreateSamplerState(false);
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

        private static SamplerState CreateSamplerState(bool linear)
        {
            var dev = ImageFramework.DirectX.Device.Get();
            var desc = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                BorderColor = new RawColor4(),
                ComparisonFunction = Comparison.Always,
                Filter = linear ? Filter.MaximumMinMagMipLinear : Filter.ComparisonMinMagMipPoint,
                MaximumAnisotropy = 0,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0
            };

            return new SamplerState(dev.Handle, desc);
        }
        public void Dispose()
        {
            DefaultBlendState?.Dispose();
            AlphaBlendState?.Dispose();
            LinearSampler?.Dispose();
            PointSampler?.Dispose();
        }
    }
}
