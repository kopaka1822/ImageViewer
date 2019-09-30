using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model
{
    /// <summary>
    /// some useful stuff for directx
    /// </summary>
    internal class DxModel : IDisposable
    {
        // constant buffer that contains info about current layer and level
        private Buffer layerLevelBuffer;

        public DxModel()
        {
            var layerLevelDesc = new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = 2 * 4,
                StructureByteStride = 0,
                Usage = ResourceUsage.Dynamic
            };
            layerLevelBuffer = new Buffer(Device.Get().Handle, layerLevelDesc);
            
        }

        public void Dispose()
        {
        }
    }
}
