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
        //private readonly SamplerState maxSampler;
        //private readonly SamplerState minSampler;

        public SmoothVolumeShader(ModelsEx models) 
            : base(models, GetPixelSource(), "SmoothVolumeShader")
        {
            var clamp = TextureAddressMode.Clamp;

            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                //AddressU = TextureAddressMode.Border,
                //AddressV = TextureAddressMode.Border,
                //AddressW = TextureAddressMode.Border,
                AddressU = clamp,
                AddressV = clamp,
                AddressW = clamp,
                Filter = Filter.MinMagMipLinear,
                BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 0.0f),
            });

            /*maxSampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = clamp,
                AddressV = clamp,
                AddressW = clamp,
                Filter = Filter.MaximumMinMagMipLinear
            });

            minSampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = clamp,
                AddressV = clamp,
                AddressW = clamp,
                Filter = Filter.MinimumMinMagMipLinear
            });*/
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
        public void Run(Matrix transform, ShaderResourceView texture, ShaderResourceView emptySpaceTex)
        {
            var v = models.ViewData;
            v.Buffer.SetData(GetCommonData(transform, models.Display.ClientAspectRatioScalar));

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
SamplerState maxSampler : register(s1);
SamplerState minSampler : register(s2);

cbuffer InfoBuffer : register(b0) {{
    {CommonShaderBufferData()}
}};
{Utility.ToSrgbFunction()}

{PixelInStruct()}

{CommonShaderFunctions()}

float4 main(PixelIn i) : SV_TARGET {{
    // color
    float3 c = 0.0;
    // transmittance
    float3 t = 1.0;

    int width, height, depth;
    tex.GetDimensions(width, height, depth);
    int3 size = uint3(width, height, depth);
    float3 fsize = float3(size);    

    const float stepsize = 4.0;
    const float invStepsize = 1.0 / stepsize;

    float3 unitRay = normalize(i.rayDir);
    float3 ray = unitRay / stepsize;

    float3 pos;    
    if(!getIntersection(origin, ray, pos)) return float4(0, 0, 0, 1);

    // convert from pixel coordinates to texel ([0, 1])
    float3 invSize = 1 / fsize;    
    
    float skipped = 0.0;
    [loop] do{{
        // determine empty space
        int skipValue = int(emptySpaceTex[clamp(int3(pos), 0, size-1)]);
        if(skipValue <= 1) {{
            float4 s = tex.SampleLevel(texSampler, pos * invSize, 0);
            // adjust alpha based on stepsize        
            s.a = 1.0 - pow(max(1.0 - s.a, 0.0), invStepsize);
            c += calcColor(s, t);

            t *= 1.0 - s.a;   
            pos += ray; // increase by stepsize
        }} else {{
            pos += (skipValue - 1) * unitRay;
        }}
    }} while(isInside(pos) && dot(t, 1) > 0.01);
   
    float4 color = float4(c, dot(t, 1.0 / 3.0));
    {ApplyColorTransform()}
    return toSrgb(color);
}}
";
        }
    }
}
