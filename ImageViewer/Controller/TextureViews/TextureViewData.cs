using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.DirectX;
using ImageFramework.Model.Export;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Models;
using SharpDX;
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

        public UploadBuffer<ViewBufferData> Buffer { get; }

        public CheckersShader Checkers { get; }
        public TextureViewData()
        {
            var dev = ImageFramework.DirectX.Device.Get();

            DefaultBlendState = CreateBlendState(false, BlendOption.One, BlendOption.Zero);
            AlphaBlendState = CreateBlendState(true, BlendOption.SourceAlpha, BlendOption.InverseSourceAlpha);

            LinearSampler = CreateSamplerState(true);
            PointSampler = CreateSamplerState(false);

            Buffer = new UploadBuffer<ViewBufferData>(1);

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
                Filter = linear ? Filter.MaximumMinMagMipLinear : Filter.MinMagMipPoint,
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
            LinearSampler?.Dispose();
            PointSampler?.Dispose();
            Buffer?.Dispose();
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
                    int mipmap = models.Export.AllowCropping ? models.Export.Mipmap : 0;
                    float cropMaxX = models.Images.GetWidth(mipmap) - 1;
                    float cropMaxY = models.Images.GetHeight(mipmap) - 1;

                    Vector4 res;
                    // crop start x
                    res.X = models.Export.CropStartX / cropMaxX;
                    res.Y = models.Export.CropEndX / cropMaxX;
                    res.Z = models.Export.CropStartY / cropMaxY;
                    res.W = models.Export.CropEndY / cropMaxY;

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
