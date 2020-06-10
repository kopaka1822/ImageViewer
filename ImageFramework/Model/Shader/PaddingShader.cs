using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Scaling;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    public class PaddingShader : IDisposable
    {
        private readonly DirectX.Shader shader;
        private readonly DirectX.Shader shader3D;
        private readonly SamplerState[] sampler;

        public PaddingShader()
        {
            shader = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(ShaderBuilder.Builder2D), "PaddingShader");
            shader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(ShaderBuilder.Builder3D), "PaddingShader3D");
            sampler = new SamplerState[]
            {
                new SamplerState(Device.Get().Handle, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Border,
                    AddressV = TextureAddressMode.Border,
                    AddressW = TextureAddressMode.Border,
                    BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 1.0f),
                    Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint
                }),
                new SamplerState(Device.Get().Handle, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Border,
                    AddressV = TextureAddressMode.Border,
                    AddressW = TextureAddressMode.Border,
                    BorderColor = new RawColor4(1.0f, 1.0f, 1.0f, 1.0f),
                    Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint
                }),
                new SamplerState(Device.Get().Handle, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Border,
                    AddressV = TextureAddressMode.Border,
                    AddressW = TextureAddressMode.Border,
                    BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 0.0f),
                    Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint
                }),
                new SamplerState(Device.Get().Handle, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Clamp,
                    AddressV = TextureAddressMode.Clamp,
                    AddressW = TextureAddressMode.Clamp,
                    Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint
                }),
            };
        }

        public enum FillMode
        {
            Black,
            White,
            Transparent,
            Clamp
        }

        public ITexture Run(ITexture src, Size3 leftPad, Size3 rightPad, FillMode fill, ScalingModel scaling, SharedModel shared)
        {
            Size3 dstSize = leftPad + rightPad + src.Size;
            int nMipmaps = src.NumMipmaps > 1 ? dstSize.MaxMipLevels : 1;

            var dst = src.Create(new LayerMipmapCount(src.NumLayers, nMipmaps), dstSize, src.Format, src.HasUaViews,
                true);

            var dev = DirectX.Device.Get();
            shared.Upload.SetData(new BufferData
            {
                Depth = dstSize.Depth,
                Offset = new Float3(leftPad) / new Float3(dstSize),
                Scale = new Float3(dstSize) / new Float3(src.Size),
            });
            
            shared.QuadShader.Bind(src.Is3D);
            if(src.Is3D) dev.Pixel.Set(shader3D.Pixel);
            else dev.Pixel.Set(shader.Pixel);
            dev.Pixel.SetSampler(0, sampler[(int)fill]);
            dev.Pixel.SetConstantBuffer(0, shared.Upload.Handle);

            foreach (var lm in src.LayerMipmap.RangeOf(LayerMipmapRange.MostDetailed))
            {
                dev.OutputMerger.SetRenderTargets(dst.GetRtView(lm));
                dev.SetViewScissors(dstSize.Width, dstSize.Height);
                dev.Pixel.SetShaderResource(0, src.GetSrView(lm));

                dev.DrawFullscreenTriangle(dstSize.Depth);
            }

            // remove bindings
            shared.QuadShader.Unbind();
            dev.Pixel.Set(null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            dev.Pixel.SetShaderResource(0, null);

            if (dst.NumMipmaps > 1)
            {
                scaling.WriteMipmaps(dst);
            }

            return dst;
        }

        public void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();
            foreach (var samplerState in sampler)
            {
                samplerState?.Dispose();
            }
        }

        struct BufferData
        {
            public Float3 Offset;
            public int Depth;
            public Float3 Scale;
        }

        private static string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} in_tex : register(t0);
SamplerState texSampler : register(s0);

cbuffer InfoBuffer : register(b0)
{{
    float3 offset;
    uint depth;
    float3 scale;
}}

struct PixelIn
{{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
#if {builder.Is3DInt}
    uint depth : SV_RenderTargetArrayIndex;
#endif
}};

#if {builder.Is3DInt}
#define TexcoordT float3
float3 toTex(float3 t) {{ return t; }}
#else
#define TexcoordT float2
float2 toTex(float3 t) {{ return t.xy; }}
#endif

float4 main(PixelIn i) : SV_TARGET
{{
    TexcoordT coord;
    // dst texture coordinate
    coord.xy = i.texcoord;
#if {builder.Is3DInt}   
     coord.z = (i.depth + 0.5f) / depth; 
#endif    

    // calc src texture coordinate
    coord = (coord - toTex(offset)) * toTex(scale);

    return in_tex.Sample(texSampler, coord);
}}
";
        }
    }
}
