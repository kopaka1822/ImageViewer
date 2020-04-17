using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class SmoothVolumeShader : VolumeShader
    {
        private readonly SamplerState sampler;

        public SmoothVolumeShader(ModelsEx models) 
            : base(models, GetPixelSource(), "SmoothVolumeShader")
        {
            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                //AddressU = TextureAddressMode.Clamp,
                //AddressV = TextureAddressMode.Clamp,
                //AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 0.0f),
            });
        }

        public override void Dispose()
        {
            sampler?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform">transformation matrix from camera space to image space (pixel coordinates)</param>
        /// <param name="screenAspect">screen aspect ratio</param>
        /// <param name="texture"></param>
        /// <param name="emptySpaceTex"></param>
        public void Run(Matrix transform, float screenAspect, ShaderResourceView texture, ShaderResourceView emptySpaceTex)
        {
            var v = models.ViewData;
            v.Buffer.SetData(GetCommonData(transform, screenAspect));

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, v.Buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetSampler(0, sampler);
            dev.Pixel.SetShaderResource(1, emptySpaceTex);

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
            UnbindShader(dev);
        }

        private static string GetPixelSource()
        {
            return $@"
Texture3D<float4> tex : register(t0);
Texture3D<uint> emptySpaceTex : register(t1);
SamplerState texSampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    {CommonShaderBufferData()}
}};
{Utility.ToSrgbFunction()}

{PixelInStruct()}

{CommonShaderFunctions()}

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = 0.0;
    color.a = 1.0; // transmittance

    int width, height, depth;
    tex.GetDimensions(width, height, depth);
    int3 size = uint3(width, height, depth);
    float3 fsize = float3(size);    

    // the height of the cube is 2.0 in world space. Width and Depth depend on aspect of height
    const float stepsize = 4.0;
    const float invStepsize = 1.0 / stepsize;

    float3 unitRay = normalize(i.rayDir);
    float3 ray = unitRay / stepsize;

    float3 pos;    
    if(!getIntersection(origin, ray, fsize, pos)) return color;

    // convert from pixel coordinates to texel ([0, 1])
    float3 invSize = 1 / fsize;    
    
    float skipped = 0.0;
    [loop] do{{
        float3 pos2 = float3(pos.xy,1-pos.z);

        //skip empty space
        pos += max(int(emptySpaceTex[clamp(int3(pos), 0, size-1)]) - 1, 0) * unitRay;

        if(!isInside(pos, fsize)) break;

        float4 s = tex.SampleLevel(texSampler, pos * invSize, 0);

        float invAlpha = pow(max(1.0 - s.a, 0.0), invStepsize);
        float alpha = 1.0 - invAlpha;
        color.rgb += color.a * alpha * s.rgb;
        color.a *= invAlpha;
        
        pos += ray;
    }} while(color.a > 0.0);
   
    {ApplyColorTransform()}
    return toSrgb(color);
}}
";
        }
    }
}
