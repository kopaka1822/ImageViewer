using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImageFramework.DirectX
{
    /// <summary>
    /// buffer in gpu memory with unordered access view
    /// </summary>
    public class GpuBuffer : IDisposable
    {
        public readonly Buffer Handle;
        public readonly UnorderedAccessView View;
        public int ElementSize { get; }
        public int ElementCount { get; }
        public int ByteSize { get; }

        public GpuBuffer(int elementSize, int elementCount)
        {
            ElementSize = elementSize;
            ElementCount = elementCount;
            ByteSize = elementSize * elementCount;

            var bufferDesc = new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                Usage = ResourceUsage.Default,
                SizeInBytes = ByteSize,
                StructureByteStride = elementSize
            };

            Handle = new Buffer(Device.Get().Handle, bufferDesc);
            View = new UnorderedAccessView(Device.Get().Handle, Handle);
        }

        public void CopyFrom<T>(UploadBuffer<T> buffer) where T : struct
        {
            Debug.Assert(buffer.ByteSize <= ByteSize);
            Device.Get().CopyBufferData(buffer.Handle, Handle, ByteSize);
        }

        public void Dispose()
        {
            View?.Dispose();
            Handle?.Dispose();
        }
    }
}
