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

        private Dictionary<int, UnorderedAccessView> views;

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

        /// <summary>
        /// returns a uav views with the specified offset
        /// </summary>
        UnorderedAccessView GetView(int offset)
        {
            Debug.Assert(offset < ElementCount);
            if(views == null) views = new Dictionary<int, UnorderedAccessView>();

            if (views.TryGetValue(offset, out var hashedView)) return hashedView;

            var newView = new UnorderedAccessView(Device.Get().Handle, Handle, new UnorderedAccessViewDescription
            {
                Dimension = UnorderedAccessViewDimension.Buffer,
                Format = Format.Unknown,
                Buffer = new UnorderedAccessViewDescription.BufferResource
                {
                    FirstElement = offset,
                    ElementCount = ElementCount - offset,
                    Flags = UnorderedAccessViewBufferFlags.None
                }
            });

            views[offset] = newView;
            return newView;
        }

        public void CopyFrom(UploadBuffer buffer)
        {
            Debug.Assert(ByteSize <= buffer.ByteSize);
            Device.Get().CopyBufferData(buffer.Handle, Handle, ByteSize);
        }

        public void Dispose()
        {
            if (views != null)
            {
                foreach (var uav in views)
                {
                    uav.Value.Dispose();
                }
            }
            View?.Dispose();
            Handle?.Dispose();
        }
    }
}
