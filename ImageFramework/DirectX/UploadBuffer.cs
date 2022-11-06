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
    public class UploadBuffer : IDisposable
    {
        public readonly Buffer Handle;
        public int ByteSize { get; }

        /// <summary>
        /// default constructor for non structured data
        /// </summary>
        public UploadBuffer(int byteSize, BindFlags binding = BindFlags.ConstantBuffer, ResourceOptionFlags resourceFlags = ResourceOptionFlags.None)
        {
            ByteSize = Utility.Utility.AlignTo(byteSize, 16);

            var bufferDesc = new BufferDescription
            {
                BindFlags = binding,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = resourceFlags,
                SizeInBytes = ByteSize,
                StructureByteStride = 0,
                Usage = ResourceUsage.Default
            };

            Handle = new Buffer(Device.Get().Handle, bufferDesc);
        }

        /// <summary>
        /// specialized constructor for structured buffers
        /// </summary>
        public UploadBuffer(int byteSize, int structureByteSize, BindFlags binding = BindFlags.ShaderResource)
        {
            ByteSize = Utility.Utility.AlignTo(byteSize, 16);

            var bufferDesc = new BufferDescription
            {
                BindFlags = binding,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                SizeInBytes = ByteSize,
                StructureByteStride = structureByteSize,
                Usage = ResourceUsage.Default
            };

            Handle = new Buffer(Device.Get().Handle, bufferDesc);
        }

        public void SetData<T>(T data) where T : struct
        {
            Debug.Assert(Marshal.SizeOf(typeof(T)) <= ByteSize);

            Device.Get().UpdateBufferData(Handle, data);
        }

        public void SetData<T>(T[] data) where T : struct
        {
            Debug.Assert(Marshal.SizeOf(typeof(T)) * data.Length <= ByteSize);
            Device.Get().UpdateBufferData(Handle, data);
        }

        public void Dispose()
        {
            Handle?.Dispose();
        }
    }
}
