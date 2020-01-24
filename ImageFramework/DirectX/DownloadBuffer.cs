using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class DownloadBuffer : IDisposable
    {
        private readonly Buffer stageBuffer;

        public int ByteSize { get; }

        public DownloadBuffer(int byteSize)
        {
            ByteSize = Utility.Utility.AlignTo(byteSize, 16);

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

            var stageDesc = new BufferDescription
            {
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = ByteSize,
                StructureByteStride = 0,
                Usage = ResourceUsage.Staging
            };

            stageBuffer = new Buffer(Device.Get().Handle, stageDesc);
        }

        public void CopyFrom(GpuBuffer buffer)
        {
            Device.Get().CopyBufferData(buffer.Handle, stageBuffer, Math.Min(ByteSize, buffer.ByteSize));
        }

        public T GetData<T>() where T : struct
        {
            var res = new T[1];

            Debug.Assert(Marshal.SizeOf(typeof(T)) <= ByteSize);
            Device.Get().GetData(stageBuffer, ref res);
            return res[0];
        }

        public T[] GetData<T>(int count) where T : struct
        {
            var res = new T[count];

            Debug.Assert(Marshal.SizeOf(typeof(T)) * count <= ByteSize);
            Device.Get().GetData(stageBuffer, ref res);
            return res;
        }

        public void Dispose()
        {
            stageBuffer?.Dispose();
        }
    }
}
