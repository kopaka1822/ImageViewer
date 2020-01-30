using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    internal class GaussShader : IDisposable
    {
        private readonly int radius;
        private readonly float variance;
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;
        private QuadShader quad;

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                             GetSource(ShaderBuilder.Builder2D), "GaussShader"));
        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                             GetSource(ShaderBuilder.Builder3D), "GaussShader"));

        internal void CompileShaders()
        {
            var s = Shader;
            s = Shader3D;
        }

        public GaussShader(int radius, float variance, QuadShader quad)
        {
            this.radius = radius;
            this.quad = quad;
            this.variance = variance;
        }

        private struct BufferData
        {
            public int Size;
            public Size3 Direction;
        }

        public void Run(ITexture src, ITexture dst, int layer, int mipmap, UploadBuffer buffer, ITextureCache cache)
        {
            Debug.Assert(src.HasSameDimensions(dst));
            Debug.Assert(cache.IsCompatibleWith(src));
            Debug.Assert(layer < src.NumLayers);
            Debug.Assert(mipmap < src.NumMipmaps);

            var srcSize = src.Size.GetMip(mipmap);

            var dev = Device.Get();
            quad.Bind(src.Is3D);
            dev.Pixel.Set(src.Is3D ? Shader3D.Pixel : Shader.Pixel);

            // execute x
            buffer.SetData(new BufferData
            {
                Size = srcSize.X,   
                Direction = new Size3(1, 0, 0)
            });
            dev.Pixel.SetConstantBuffer(0, buffer.Handle);
            dev.Pixel.SetShaderResource(0, src.GetSrView(layer, mipmap));
            var tmp1 = cache.GetTexture();
            ITexture tmp2 = null;
            dev.OutputMerger.SetRenderTargets(tmp1.GetRtView(layer, mipmap));
            dev.SetViewScissors(srcSize.Width, srcSize.Height);

            dev.DrawFullscreenTriangle(srcSize.Depth);
            UnbindResources(dev);

            // execute y
            buffer.SetData(new BufferData
            {
                Size = srcSize.Y,
                Direction = new Size3(0, 1, 0)
            });
            dev.Pixel.SetConstantBuffer(0, buffer.Handle);
            dev.Pixel.SetShaderResource(0, tmp1.GetSrView(layer, mipmap));
            if (src.Is3D)
            {
                tmp2 = cache.GetTexture();
                dev.OutputMerger.SetRenderTargets(tmp2.GetRtView(layer, mipmap));
                dev.SetViewScissors(srcSize.Width, srcSize.Height);

                dev.DrawFullscreenTriangle(srcSize.Depth);
                UnbindResources(dev);

                // execute z
                buffer.SetData(new BufferData
                {
                    Size = srcSize.Z,
                    Direction = new Size3(0, 0, 1)
                });
                dev.Pixel.SetConstantBuffer(0, buffer.Handle);
                dev.Pixel.SetShaderResource(0, tmp2.GetSrView(layer, mipmap));
            }

            // bind final target
            dev.OutputMerger.SetRenderTargets(dst.GetRtView(layer, mipmap));
            dev.SetViewScissors(srcSize.Width, srcSize.Height);

            dev.DrawFullscreenTriangle(srcSize.Depth);
            UnbindResources(dev);
            cache.StoreTexture(tmp1);
            if(tmp2 != null) cache.StoreTexture(tmp2);

            quad.Unbind();
        }

        private void UnbindResources(Device dev)
        {
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
        }

        public void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();
        }

        private string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);

cbuffer InputBuffer : register(b0) {{
    int size;
    {builder.IntVec} dir; // filter direction
}};

float kernel(int offset) {{
    return exp(-offset * offset / {variance});
}}

struct PixelIn
{{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
#if {builder.Is3DInt}
    uint depth : SV_RenderTargetArrayIndex;
#endif
}};

bool isInside(int x) {{
    return x >= 0 && x < size;
}}

float4 main(PixelIn pin) : SV_TARGET 
{{
    {builder.IntVec} id;
    id.xy = int2(pin.projPos.xy);
#if {builder.Is3DInt}
    id.z = pin.depth;
#endif
    float weightSum = 0.0;
    float4 pixelSum = 0.0;

    {builder.IntVec} pos = id - {radius} * dir;
    int icenter = dot(id, dir);

    [unroll] for(int i = -{radius}; i <= {radius}; ++i) 
    {{
        [flatten] if(isInside(icenter + i)){{
            float w = kernel(i);
            weightSum += w;
            pixelSum += src_image[pos];
        }}
        pos += dir;
    }}

    return pixelSum / weightSum;
}}
";
        }
    }
}
