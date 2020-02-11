using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    /// <summary>
    /// shader that can convert between latlong maps and cubemaps
    /// </summary>
    public class ConvertPolarShader : IDisposable
    {
        private readonly QuadShader quad;
        private readonly DirectX.Shader toCube;
        private readonly DirectX.Shader toLatLong;
        private readonly SamplerState sampler;

        public ConvertPolarShader(QuadShader quad)
        {
            this.quad = quad;
            toCube = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetCubeSource(), "ConvertToCube");
            toLatLong = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetLatLongSource(), "CovertToLatLong");
            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                Filter = SharpDX.Direct3D11.Filter.Anisotropic,
                MaximumAnisotropy = 16
            });
        }

        // converts a lat long texture to a cubemap
        public TextureArray2D ConvertToCube(TextureArray2D latlong, int resolution)
        {
            Debug.Assert(latlong.NumLayers == 1);

            var dst = new TextureArray2D(new LayerMipmapCount(6, 1), new Size3(resolution, resolution), Format.R32G32B32A32_Float, false);

            var dev = Device.Get();
            quad.Bind(false);
            dev.Pixel.Set(toCube.Pixel);

            dev.Pixel.SetShaderResource(0, latlong.GetSrView(LayerMipmapSlice.Mip0));
            dev.Pixel.SetSampler(0, sampler);
            dev.OutputMerger.SetRenderTargets(null, 
                dst.GetRtView(new LayerMipmapSlice(0, 0)), 
                dst.GetRtView(new LayerMipmapSlice(1, 0)),
                dst.GetRtView(new LayerMipmapSlice(2, 0)),
                dst.GetRtView(new LayerMipmapSlice(3, 0)),
                dst.GetRtView(new LayerMipmapSlice(4, 0)),
                dst.GetRtView(new LayerMipmapSlice(5, 0)));
            dev.SetViewScissors(resolution, resolution);
            dev.DrawQuad();

            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetSampler(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();

            return dst;
        }

        /// converts a cubemap texture to a lat long map
        public TextureArray2D ConvertToLatLong(TextureArray2D cube, int resolution)
        {
            Debug.Assert(cube.NumLayers == 6);

            var dst = new TextureArray2D(LayerMipmapCount.One, new Size3(resolution, Math.Max(resolution / 2, 1)), Format.R32G32B32A32_Float, false);

            var dev = Device.Get();
            quad.Bind(false);
            dev.Pixel.Set(toLatLong.Pixel);

            var dim = dst.Size;
            dev.Pixel.SetShaderResource(0, cube.GetCubeView(0));
            dev.Pixel.SetSampler(0, sampler);
            dev.OutputMerger.SetRenderTargets(dst.GetRtView(LayerMipmapSlice.Mip0));
            dev.SetViewScissors(dim.Width, dim.Height);
            dev.DrawQuad();

            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetSampler(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();

            return dst;
        }

        public void Dispose()
        {
            toCube?.Dispose();
            toLatLong?.Dispose();
            sampler?.Dispose();
        }

        private static string GetCubeSource()
        {
            return @"
Texture2D<float4> tex : register(t0);
SamplerState texSampler : register(s0);

struct PixelIn {
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION; 
};

struct PixelOut
{
    float4 xpos : SV_Target0;
    float4 xneg : SV_Target1;
    float4 ypos : SV_Target2;
    float4 yneg : SV_Target3;
    float4 zpos : SV_Target4;
    float4 zneg : SV_Target5;
};

float4 getColor(float3 rayDir) {
    const float PI = 3.14159265358979323846264;
    float2 polarDirection;
    float3 normalizedDirection = normalize(rayDir);
    // t computation
    polarDirection.y = acos(normalizedDirection.y) / PI;
    // s computation
    polarDirection.x = normalizedDirection.x == 0 ? 
        PI / 2.0 * sign(normalizedDirection.z) :
        atan2(normalizedDirection.z, normalizedDirection.x);
    polarDirection.x = polarDirection.x / (2.0 * PI) + 0.25;
    if(polarDirection.x < 0.0) polarDirection.x += 1.0;
    
    return tex.Sample(texSampler, polarDirection);
}

PixelOut main(PixelIn i) {
    PixelOut o;
    // scale to [-1, 1]
    float2 dir = i.texcoord * 2.0 - 1.0;
    o.xpos = getColor(float3(-1.0, -dir.y, -dir.x));
    o.xneg = getColor(float3(1.0, -dir.y, dir.x));
    o.ypos = getColor(float3(-dir.x, 1.0, dir.y));    
    o.yneg = getColor(float3(-dir.x, -1.0, -dir.y));
    o.zpos = getColor(float3(-dir.x, -dir.y, 1.0));
    o.zneg = getColor(float3(dir.x, -dir.y, -1.0));

    return o;
}
";
        }

        private static string GetLatLongSource()
        {
            return @"
TextureCube<float4> tex : register(t0);
SamplerState texSampler : register(s0);

struct PixelIn {
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

float4 main(PixelIn i) : SV_TARGET {
    const float PI = 3.14159265358979323846264;

    // convert texcoord to direction
    float3 direction;
    // spherical coordinates
    float theta = i.texcoord.y * PI;
    float phi = -(i.texcoord.x + 0.25) * 2.0 * PI;
    direction.y = cos(theta);
    direction.x = sin(theta) * cos(phi);
    direction.z = sin(theta) * sin(phi);

    return tex.Sample(texSampler, direction);
}
";
        }
    }
}
