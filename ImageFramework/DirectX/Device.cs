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

        private static Device instance = new Device();
        private Device()
        {
            DeviceCreationFlags flags = DeviceCreationFlags.DisableGpuTimeout;
#if DEBUG
            flags |= DeviceCreationFlags.Debug;
#endif
            Handle = new SharpDX.Direct3D11.Device(DriverType.Hardware, flags, new FeatureLevel[]{FeatureLevel.Level_11_1});

            // obtain the factory that created the device
            var obj = Handle.QueryInterface<SharpDX.DXGI.Device>();
            var adapter = obj.Adapter;
            FactoryHandle = adapter.GetParent<SharpDX.DXGI.Factory>();
            context = Handle.ImmediateContext;

            SetDefaults();
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

        public void GetData(Texture2D res, int subresource, int width, int height, IntPtr dst, uint size)
        {
            Debug.Assert(IO.SupportedFormats.Contains(res.Description.Format));
            int pixelSize = 4;
            if (res.Description.Format == Format.R32G32B32A32_Float)
                pixelSize = 16;

            // verify expected size
            Debug.Assert((uint)(height * width * pixelSize) == size);

            var data = context.MapSubresource(res, subresource, MapMode.Read, MapFlags.None);
            int rowSize = width * pixelSize;

            for (int curY = 0; curY < height; ++curY)
            {
                Dll.CopyMemory(dst, data.DataPointer, (uint)rowSize);
                dst += rowSize;
                data.DataPointer += data.RowPitch;
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
        public unsafe Color[] GetColorData(Texture2D res, int subresource, int width, int height)
        {
            Debug.Assert(IO.SupportedFormats.Contains(res.Description.Format));

            if (res.Description.Format == Format.R32G32B32A32_Float)
            {
                var tmp = GetData(res, subresource, width, height, 4 * 4);
                var result = new Color[width * height];
                fixed (byte* pBuffer = tmp)
                {
                    for (int i = 0; i < result.Length; i++)
                        result[i] = ((Color*)pBuffer)[i];
                }

                return result;
            }
            else
            {
                var tmp = GetData(res, subresource, width, height, 4);
                var result = new Color[width * height];
                bool isSigned = res.Description.Format == Format.R8G8B8A8_SNorm;
                bool isSrgb = res.Description.Format == Format.R8G8B8A8_UNorm_SRgb;
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

        public void Dispatch(int x, int y, int z = 1)
        {
            context.Dispatch(x, y, z);
        }

        public VertexShaderStage Vertex => context.VertexShader;
        public PixelShaderStage Pixel => context.PixelShader;
        public ComputeShaderStage Compute => context.ComputeShader;
        public InputAssemblerStage InputAssembler => context.InputAssembler;
        public OutputMergerStage OutputMerger => context.OutputMerger;
        public RasterizerStage Rasterizer => context.Rasterizer;
        public DeviceContext ContextHandle => context;

        public void Dispose()
        {
            context?.Dispose();
            Handle?.Dispose();
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

        public void DrawQuad()
        {
            context.InputAssembler.InputLayout = null;
            context.Draw(4, 0);
        }

        public void UpdateBufferData<T>(Buffer buffer, T data) where T : struct
        {
            context.UpdateSubresource(ref data, buffer);
        }

        public void UpdateBufferData<T>(Buffer buffer, T[] data) where T : struct
        {
            context.UpdateSubresource(data, buffer);
        }

        public void EndQuery(Query query)
        {
            context.End(query);
        }

        /// <summary>
        /// true if the event is completed
        /// </summary>
        /// <param name="query"></param>
        public bool GetQueryEventData(Query query)
        {
            return context.GetData<int>(query) != 0;
        }
    }
}
