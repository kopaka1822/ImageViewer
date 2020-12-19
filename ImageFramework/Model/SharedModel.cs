using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Query;
using ImageFramework.Model.Shader;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;

namespace ImageFramework.Model
{
    /// <summary>
    /// data that is usually used by multiple models
    /// </summary>
    public class SharedModel : IDisposable
    {
        public MitchellNetravaliScaleShader ScaleShader { get; }
        public QuadShader QuadShader { get; } = new QuadShader();
        public ConvertFormatShader Convert { get; }
        public UploadBuffer Upload { get; }
        public DownloadBuffer Download { get; }

        private PaddingShader padding = null;
        public PaddingShader Padding => padding ?? (padding = new PaddingShader());
        internal SyncQuery Sync { get; }
        public SamplerState LinearSampler { get; }

        public SamplerState PointSampler { get; }

        public SharedModel()
        {
            Upload = new UploadBuffer(256); // big enough for 4 matrix4
            Download = new DownloadBuffer(256);
            ScaleShader = new MitchellNetravaliScaleShader(QuadShader, Upload);
            Convert = new ConvertFormatShader(QuadShader, Upload);
            Sync = new SyncQuery();
            LinearSampler = CreateSamplerState(true);
            PointSampler = CreateSamplerState(false);
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
                Filter = linear ? SharpDX.Direct3D11.Filter.MinMagLinearMipPoint : SharpDX.Direct3D11.Filter.MinMagMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0
            };

            return new SamplerState(dev.Handle, desc);
        }

        public void Dispose()
        {
            Convert?.Dispose();
            ScaleShader?.Dispose();
            QuadShader?.Dispose();
            Upload?.Dispose();
            Sync?.Dispose();
            padding?.Dispose();
            LinearSampler?.Dispose();
            PointSampler?.Dispose();
        }
    }
}
