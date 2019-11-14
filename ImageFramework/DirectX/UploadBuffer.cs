using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImageFramework.DirectX
{
    /// <summary>
    /// this should be used to upload data to the gpu or for constant buffers that update frequently
    /// </summary>
    public class UploadBuffer<T> : IDisposable where T : struct
    {
        public readonly Buffer Handle;

        public int ElementCount { get; }
        public int ByteSize { get; }

        public UploadBuffer(int numElements = 1)
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            ElementCount = numElements;
            ByteSize = elementSize * ElementCount;

            var bufferDesc = new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utility.Utility.AlignTo(ByteSize, 16),
                StructureByteStride = elementSize,
                Usage = ResourceUsage.Default
            };

            Handle = new Buffer(Device.Get().Handle, bufferDesc);
        }

        public void SetData(T data)
        {
            Debug.Assert(ElementCount == 1);
            Device.Get().UpdateBufferData(Handle, data);
        }

        public void SetData(T[] data)
        {
            Debug.Assert(data.Length == ElementCount);
            Device.Get().UpdateBufferData(Handle, data);
        }

        public void Dispose()
        {
            Handle?.Dispose();
        }
    }
}
