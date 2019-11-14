using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImageFramework.DirectX
{
    /// <summary>
    /// buffer to read back data from the gpu to cpu
    /// </summary>
    public class DownloadBuffer<T> : IDisposable where T : struct
    {
        private readonly Buffer gpuBuffer;
        private readonly Buffer stageBuffer;
        public UnorderedAccessView Handle;

        public int ByteSize { get; }

        public DownloadBuffer()
        {
            var elementSize = Marshal.SizeOf(typeof(T));
            ByteSize = elementSize;

            var bufferDesc = new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = Utility.Utility.AlignTo(ByteSize, 16),
                StructureByteStride = elementSize,
                Usage = ResourceUsage.Default
            };

            gpuBuffer = new Buffer(Device.Get().Handle, bufferDesc);

            var viewDesc = new UnorderedAccessViewDescription
            {
                Dimension = UnorderedAccessViewDimension.Buffer,
                Format = Format.Unknown,
                Buffer = new UnorderedAccessViewDescription.BufferResource
                {
                    ElementCount = 1,
                    FirstElement = 0,
                    Flags = UnorderedAccessViewBufferFlags.None
                }
            };

            Handle = new UnorderedAccessView(Device.Get().Handle, gpuBuffer, viewDesc);

            var stageDesc = new BufferDescription
            {
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = Utility.Utility.AlignTo(ByteSize, 16),
                StructureByteStride = elementSize,
                Usage = ResourceUsage.Staging
            };

            stageBuffer = new Buffer(Device.Get().Handle, stageDesc);
        }

        public void CopyFrom(GpuBuffer buffer)
        {
            Device.Get().CopyBufferData(buffer.Handle, gpuBuffer, ByteSize);
        }

        public T GetData()
        {
            // transfer data to the staging buffer
            Device.Get().CopyResource(gpuBuffer, stageBuffer);

            T[] res = new T[1];

            var elementSize = Marshal.SizeOf(typeof(T));
            Device.Get().GetData(stageBuffer, ref res);
            return res[0];
        }

        public void Dispose()
        {
            Handle?.Dispose();
            gpuBuffer?.Dispose();
            stageBuffer?.Dispose();
        }
    }
}
