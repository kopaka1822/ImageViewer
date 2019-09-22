using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Buffer = System.Buffer;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Resource = SharpDX.Direct3D11.Resource;

namespace ImageFramework.DirectX
{
    /// <summary>
    /// singleton of the d3d11 device
    /// </summary>
    public class Device : IDisposable
    {
        private SharpDX.Direct3D11.DeviceContext context = null;
        public SharpDX.Direct3D11.Device Handle { get; }
        public SharpDX.DXGI.Factory FactoryHandle { get; }

        private static Device instance = new Device();
        private Device()
        {
            DeviceCreationFlags flags = DeviceCreationFlags.DisableGpuTimeout;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif
            Handle = new SharpDX.Direct3D11.Device(DriverType.Hardware, flags, new FeatureLevel[]{FeatureLevel.Level_11_0});

            // obtain the factory that created the device
            var obj = Handle.QueryInterface<SharpDX.DXGI.Device>();
            var adapter = obj.Adapter;
            FactoryHandle = adapter.GetParent<SharpDX.DXGI.Factory>();
            context = Handle.ImmediateContext;
        }

        public static Device Get()
        {
            return instance;
        }

        public void ClearRenderTargetView(RenderTargetView view, RawColor4 color)
        {
            context.ClearRenderTargetView(view, color);
        }

        public void CopyResource(Resource src, Resource dst)
        {
            context.CopyResource(src, dst);
        }

        /// <summary>
        /// copies entire subresource
        /// </summary>
        public void CopySubresource(Resource src, Resource dst, int srcSubresource, int dstSubresource, int width, int height)
        {
            context.CopySubresourceRegion(src, srcSubresource, new ResourceRegion(0, 0, 0, width, height, 1), 
                dst, dstSubresource);
        }

        public void GenerateMips(ShaderResourceView res)
        {
            context.GenerateMips(res);
        }

        public byte[] GetData(Resource res, int subresource, int width, int height, int pixelByteSize)
        {
            var result = new byte[width * height * pixelByteSize];
            var data = context.MapSubresource(res, subresource, MapMode.Read, MapFlags.None);
            int srcOffset = 0;
            int dstOffset = 0;
            int rowSize = width * pixelByteSize;
            Debug.Assert(rowSize <= data.RowPitch);

            for (int curY = 0; curY < height; ++curY)
            {
                Marshal.Copy(data.DataPointer + srcOffset, result, dstOffset, rowSize);

                srcOffset += data.RowPitch;
                dstOffset += rowSize;
            }

            context.UnmapSubresource(res, subresource);

            return result;
        }

        /// <summary>
        /// only works if the format is R32G32B32A32 FLOAT
        /// </summary>
        public unsafe Color[] GetColorData(Texture2D res, int subresource, int width, int height)
        {
            Debug.Assert(res.Description.Format == Format.R32G32B32A32_Float);

            var tmp = GetData(res, subresource, width, height, 4 * 4);
            var result = new Color[width * height];
            fixed (byte* pBuffer = tmp)
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = ((Color*)pBuffer)[i];
            }

            return result;
        }

        public void Dispose()
        {
            context?.Dispose();
            Handle?.Dispose();
        }
    }
}
