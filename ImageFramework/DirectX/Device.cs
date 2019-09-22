using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
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

        public void Dispose()
        {
            context?.Dispose();
            Handle?.Dispose();
        }
    }
}
