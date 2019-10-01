using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model
{
    internal struct LayerLevelData
    {
        public int Layer;
        public int Level;
    }
    
    /// <summary>
    /// some useful stuff for directx
    /// </summary>
    internal class DxModel : IDisposable
    {
        internal UploadBuffer<LayerLevelData> LayerLevelBuffer { get; }

        public DxModel()
        {
            LayerLevelBuffer = new UploadBuffer<LayerLevelData>();
            
        }

        public void Dispose()
        {
            LayerLevelBuffer?.Dispose();
        }
    }
}
