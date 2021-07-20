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
        
        /// <summary>
        /// 256 bytes CPU to GPU buffer
        /// </summary>
        public UploadBuffer Upload { get; }
        
        /// <summary>
        /// 256 bytes GPU to CPU buffer
        /// </summary>
        public DownloadBuffer Download { get; }
        
        /// <summary>
        /// 256 byte GPU staging buffer with a 4byte per element structure (up to 64 elements)
        /// </summary>
        //public GpuBuffer Gpu4ByteBuffer { get; }

        private PaddingShader padding = null;
        public PaddingShader Padding => padding ?? (padding = new PaddingShader());
        internal SyncQuery Sync { get; }
        public SamplerState LinearSampler { get; }

        public SamplerState PointSampler { get; }

        public SharedModel()
        {
            Upload = new UploadBuffer(256); // big enough for 4 matrix4
            Download = new DownloadBuffer(256);
            //Gpu4ByteBuffer = new GpuBuffer(4, 64); // 64 byte structured with 4 byte elements
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
            Download?.Dispose();
            //Gpu4ByteBuffer?.Dispose();
            Sync?.Dispose();
            padding?.Dispose();
            LinearSampler?.Dispose();
            PointSampler?.Dispose();
        }
    }
}
