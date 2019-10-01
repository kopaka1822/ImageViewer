using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImageFramework.DirectX
{
    /// <summary>
    /// buffer to read back data from the gpu to cpu
    /// </summary>
    internal class DownloadBuffer<T> : IDisposable where T : struct
    {
        private readonly Buffer buffer;

        public DownloadBuffer()
        {
            var elementSize = Marshal.SizeOf(typeof(T));

            var bufferDesc = new BufferDescription
            {
                BindFlags = BindFlags.UnorderedAccess,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = elementSize,
                StructureByteStride = elementSize,
                Usage = ResourceUsage.Staging
            };

            buffer = new Buffer(Device.Get().Handle, bufferDesc);
        }

        public T GetData()
        {
            T[] res = new T[1];

            var elementSize = Marshal.SizeOf(typeof(T));
            Device.Get().GetData(buffer, ref res);
            return res[0];
        }

        public void Dispose()
        {
            buffer?.Dispose();
        }
    }
}
