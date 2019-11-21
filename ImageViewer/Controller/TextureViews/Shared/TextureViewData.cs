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
        public TextureViewData(SharedModel shared)
        {
            var dev = ImageFramework.DirectX.Device.Get();

            DefaultBlendState = CreateBlendState(false, BlendOption.One, BlendOption.Zero);
            AlphaBlendState = CreateBlendState(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);
            AlphaDarkenState = CreateBlendState(true, BlendOption.One, BlendOption.SourceAlpha);

            LinearSampler = CreateSamplerState(true);
            PointSampler = CreateSamplerState(false);

            Buffer = shared.Upload;

            Checkers = new CheckersShader();
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

        public Vector4 GetCrop(ModelsEx models, int layer)
        {
            if (models.Export.Layer != -1) // only single layer
            {
                // darken due to layer mismatch?
                if (models.Export.IsExporting && models.Export.Layer != layer)
                {
                    // everything is gray
                    return Vector4.Zero;
                }

                if (models.Export.UseCropping && (models.Export.IsExporting || models.Display.ShowCropRectangle))
                {
                    int mipmap = Math.Max(models.Export.Mipmap, 0);
                    float cropMaxX = models.Images.GetWidth(mipmap);
                    float cropMaxY = models.Images.GetHeight(mipmap);

                    Vector4 res;
                    // crop start x
                    res.X = models.Export.CropStartX / cropMaxX;
                    res.Y = (models.Export.CropEndX + 1) / cropMaxX;
                    res.Z = models.Export.CropStartY / cropMaxY;
                    res.W = (models.Export.CropEndY + 1) / cropMaxY;

                    return res;
                }
            }

            // nothing is gray
            return new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        }

        public SamplerState GetSampler(bool displayLinearInterpolation)
        {
            return displayLinearInterpolation ? LinearSampler : PointSampler;
        }
    }
}
