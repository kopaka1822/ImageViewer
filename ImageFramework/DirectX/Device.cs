using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Buffer = SharpDX.Direct3D11.Buffer;
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

        public bool SupportsDouble { get; }

        public event EventHandler DeviceDispose;

        private static Device instance = new Device();
        private Device()
        {
            DeviceCreationFlags flags = DeviceCreationFlags.DisableGpuTimeout
                 | DeviceCreationFlags.BgraSupport;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif
            // create 11.1 if possible (disable gpu timeout) but 11.0 is also fine
            Handle = new SharpDX.Direct3D11.Device(DriverType.Hardware, flags, new FeatureLevel[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0 });

            // obtain the factory that created the device
            var obj = Handle.QueryInterface<SharpDX.DXGI.Device>();
            var adapter = obj.Adapter;
            FactoryHandle = adapter.GetParent<SharpDX.DXGI.Factory>();
            context = Handle.ImmediateContext;

            SetDefaults();
            SupportsDouble = Handle.CheckFeatureSupport(SharpDX.Direct3D11.Feature.ShaderDoubles);
        }

        // it is probably a low end device if feature level 11.1 was not available
        public bool IsLowEndDevice => Handle.FeatureLevel == FeatureLevel.Level_11_0;

        // resource size limits for textures
        public static readonly int MAX_TEXTURE_2D_DIMENSION = 16384;
        public static readonly int MAX_TEXTURE_2D_ARRAY_DIMENSION = 2048;
        public static readonly int MAX_TEXTURE_3D_DIMENSION = 2048;

        public static Device Get()
        {
            Debug.Assert(instance != null);
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
        public void CopySubresource(Resource src, Resource dst, int srcSubresource, int dstSubresource, Size3 size)
        {
            context.CopySubresourceRegion(src, srcSubresource, new ResourceRegion(0, 0, 0, size.Width, size.Height, size.Depth),
                dst, dstSubresource);
        }
        public byte[] GetData(Resource res, int subresource, Size3 size, int pixelByteSize)
        {
            var result = new byte[size.Product * pixelByteSize];
            var data = context.MapSubresource(res, subresource, MapMode.Read, MapFlags.None);
            int srcOffset = 0;
            int dstOffset = 0;
            int rowSize = size.Width * pixelByteSize;
            int sliceOffset = data.SlicePitch - data.RowPitch * size.Height;
            Debug.Assert(rowSize <= data.RowPitch);

            for (int curZ = 0; curZ < size.Depth; ++curZ)
            {
                for (int curY = 0; curY < size.Height; ++curY)
                {
                    Marshal.Copy(data.DataPointer + srcOffset, result, dstOffset, rowSize);

                    srcOffset += data.RowPitch;
                    dstOffset += rowSize;
                }
                srcOffset += sliceOffset;
            }

            context.UnmapSubresource(res, subresource);

            return result;
        }

        public void GetData(Resource res, Format format, int subresource, Size3 dim, IntPtr dst, uint size)
        {
            Debug.Assert(IO.SupportedFormats.Contains(format) || format == Format.R8_UInt);
            int pixelSize = 4;
            if (format == Format.R32G32B32A32_Float)
                pixelSize = 16;
            if (format == Format.R8_UInt)
                pixelSize = 1;

            // verify expected size
            Debug.Assert((uint)(dim.Product * pixelSize) == size);

            var data = context.MapSubresource(res, subresource, MapMode.Read, MapFlags.None);
            int rowSize = dim.Width * pixelSize;
            int sliceOffset = data.SlicePitch - data.RowPitch * dim.Height;

            for (int curZ = 0; curZ < dim.Depth; ++curZ)
            {
                for (int curY = 0; curY < dim.Height; ++curY)
                {
                    Dll.CopyMemory(dst, data.DataPointer, (uint)rowSize);
                    dst += rowSize;
                    data.DataPointer += data.RowPitch;
                }
                data.DataPointer += sliceOffset;
            }

            context.UnmapSubresource(res, subresource);
        }

        /// <summary>
        /// gets data from the buffer (should be staging buffer) and puts the data into the array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="b"></param>
        /// <param name="dst"></param>
        public void GetData<T>(Buffer b, ref T[] dst) where T : struct
        {
            var data = context.MapSubresource(b, 0, MapMode.Read, MapFlags.None);

            var elementSize = Marshal.SizeOf(typeof(T));

            for (int i = 0; i < dst.Length; ++i)
            {
                dst[i] = Marshal.PtrToStructure<T>(data.DataPointer);
                data.DataPointer += elementSize;
            }

            context.UnmapSubresource(b, 0);
        }

        /// <summary>
        /// gets color data, only works for the 4 supported formats.
        /// Mainly for debug purposes
        /// </summary>
        /// <returns></returns>
        public unsafe Color[] GetColorData(Resource res, Format format, int subresource, Size3 size)
        {
            Debug.Assert(IO.SupportedFormats.Contains(format) || format == Format.R8_UInt);

            if (format == Format.R32G32B32A32_Float)
            {
                var tmp = GetData(res, subresource, size, 4 * 4);
                var result = new Color[size.Product];
                fixed (byte* pBuffer = tmp)
                {
                    for (int i = 0; i < result.Length; i++)
                        result[i] = ((Color*)pBuffer)[i];
                }

                return result;
            }
            else if (format == Format.R8_UInt)
            {
                var tmp = GetData(res, subresource, size, 1);
                var result = new Color[size.Product];
                fixed (byte* pBuffer = tmp)
                {
                    for (int dst = 0, src = 0; dst < result.Length; ++dst, src += 1)
                    {
                        result[dst] = new Color((float)(pBuffer[src]), 0.0f, 0.0f);
                    }
                }
                return result;

            }


            else
            {
                var tmp = GetData(res, subresource, size, 4);
                var result = new Color[size.Product];
                bool isSigned = format == Format.R8G8B8A8_SNorm;
                bool isSrgb = format == Format.R8G8B8A8_UNorm_SRgb;
                fixed (byte* pBuffer = tmp)
                {
                    for (int dst = 0, src = 0; dst < result.Length; ++dst, src += 4)
                    {
                        result[dst] = new Color(pBuffer[src], pBuffer[src + 1], pBuffer[src + 2], pBuffer[src + 3], isSigned);
                        if (isSrgb)
                            result[dst] = result[dst].FromSrgb();
                    }
                }

                return result;
            }
        }

        public static readonly int DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION = 65535;

        public void Dispatch(int x, int y, int z = 1)
        {
            // test agains feature level 11.0 specs
            Debug.Assert(x <= DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION);
            Debug.Assert(y <= DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION);
            Debug.Assert(z <= DISPATCH_MAX_THREAD_GROUPS_PER_DIMENSION);
            context.Dispatch(x, y, z);
        }

        public VertexShaderStage Vertex => context.VertexShader;
        public GeometryShaderStage Geometry => context.GeometryShader;
        public PixelShaderStage Pixel => context.PixelShader;
        public ComputeShaderStage Compute => context.ComputeShader;
        public InputAssemblerStage InputAssembler => context.InputAssembler;
        public OutputMergerStage OutputMerger => context.OutputMerger;
        public RasterizerStage Rasterizer => context.Rasterizer;
        public DeviceContext ContextHandle => context;

        public void Dispose()
        {
            OnDeviceDispose();
            context?.Dispose();
            Handle?.Dispose();
            instance = null;
        }

        private void SetDefaults()
        {
            InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;

            var desc = new RasterizerStateDescription
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                DepthBiasClamp = 0,
                FillMode = FillMode.Solid,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = false,
                IsFrontCounterClockwise = true,
                IsMultisampleEnabled = false,
                IsScissorEnabled = true,
                SlopeScaledDepthBias = 0.0f
            };
            var defaultState = new RasterizerState(Handle, desc);

            context.Rasterizer.State = defaultState;
        }

        /// <summary>
        /// sets viewport and scissors
        /// </summary>
        public void SetViewScissors(int width, int height)
        {
            Rasterizer.SetViewport(0.0f, 0.0f, (float)width, (float)height);
            Rasterizer.SetScissorRectangle(0, 0, width, height);
        }

        public FormatSupport CheckFormatSupport(Format f)
        {
            return Handle.CheckFormatSupport(f);
        }

        public void DrawQuad(int count = 1)
        {
            context.InputAssembler.InputLayout = null;
            context.Draw(4 * count, 0);
        }

        public void DrawFullscreenTriangle(int count = 1)
        {
            context.InputAssembler.InputLayout = null;
            InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.Draw(3 * count, 0);
            InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        }

        public void UpdateBufferData<T>(Buffer buffer, T data) where T : struct
        {
            context.UpdateSubresource(ref data, buffer);
        }

        public void UpdateBufferData<T>(Buffer buffer, T[] data) where T : struct
        {
            context.UpdateSubresource(data, buffer);
        }

        public void EndQuery(SharpDX.Direct3D11.Query query)
        {
            context.End(query);
        }

        /// <summary>
        /// true if the event is completed
        /// </summary>
        /// <param name="query"></param>
        public bool GetQueryEventData(SharpDX.Direct3D11.Query query)
        {
            return context.GetData<int>(query, AsynchronousFlags.DoNotFlush) != 0;
        }

        public void Flush()
        {
            context.Flush();
        }

        public void CopyBufferData(Buffer src, Buffer dst, int size, int srcOffset = 0, int dstOffstet = 0)
        {
            context.CopySubresourceRegion(src, 0, new ResourceRegion(srcOffset, 0, 0, srcOffset + size, 1, 1),
                dst, 0, dstOffstet, 0, 0);
        }

        public void Begin(Asynchronous ass)
        {
            context.Begin(ass);
        }

        public void End(Asynchronous ass)
        {
            context.End(ass);
        }

        protected virtual void OnDeviceDispose()
        {
            DeviceDispose?.Invoke(this, EventArgs.Empty);
        }
    }
}
