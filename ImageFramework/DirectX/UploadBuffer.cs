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
    internal class UploadBuffer<T> : IDisposable where T : struct
    {
        public readonly Buffer Handle;
        private readonly int elementCount;

        public UploadBuffer(int numElements = 1)
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            elementCount = numElements;

            var bufferDesc = new BufferDescription
            {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = elementSize * elementCount,
                StructureByteStride = elementSize,
                Usage = ResourceUsage.Dynamic
            };

            Handle = new Buffer(Device.Get().Handle, bufferDesc);
        }

        public void SetData(T data)
        {
            Debug.Assert(elementCount == 1);
            Device.Get().UpdateBufferData(Handle, data);
        }

        public void SetData(T[] data)
        {
            Debug.Assert(data.Length == elementCount);
            Device.Get().UpdateBufferData(Handle, data);
        }

        public void Dispose()
        {
            Handle?.Dispose();
        }
    }
}
