using System;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace ImageViewer.Controller.TextureViews.Shared
{
    public class TextureViewData : IDisposable
    {
        public BlendState DefaultBlendState { get; }

        // src = alpha, dst = 1 - alpha
        public BlendState AlphaBlendState { get; }

        // src = 1, dst = alpha
        public BlendState AlphaDarkenState { get; }

        public SamplerState LinearSampler { get; }

        public SamplerState PointSampler { get; }

        public UploadBuffer Buffer { get; }

        public CheckersShader Checkers { get; }

        private readonly ModelsEx models;

        public TextureViewData(ModelsEx models)
        {
            this.models = models;
            var dev = ImageFramework.DirectX.Device.Get();

            DefaultBlendState = CreateBlendState(false, BlendOption.One, BlendOption.Zero);
            AlphaBlendState = CreateBlendState(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);
            AlphaDarkenState = CreateBlendState(true, BlendOption.One, BlendOption.SourceAlpha);

            LinearSampler = CreateSamplerState(true);
            PointSampler = CreateSamplerState(false);

            Buffer = models.SharedModel.Upload;

            Checkers = new CheckersShader(models);
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
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = new RawColor4(),
                ComparisonFunction = Comparison.Never,
                Filter = linear ? Filter.MinMagLinearMipPoint : Filter.MinMagMipPoint,
                MaximumAnisotropy = 1,
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
            AlphaDarkenState?.Dispose();
            LinearSampler?.Dispose();
            PointSampler?.Dispose();
            Checkers?.Dispose();
        }

        public SamplerState GetSampler(bool displayLinearInterpolation)
        {
            return displayLinearInterpolation ? LinearSampler : PointSampler;
        }

        /// <summary>
        /// Returns the linear or point sampler based on Display.LinearInterpolation
        /// </summary>
        public SamplerState GetSampler()
        {
            return GetSampler(models.Display.LinearInterpolation);
        }
    }
}
