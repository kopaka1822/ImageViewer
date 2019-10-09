using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class PolarViewShader : ViewShader
    {
        public PolarViewShader() 
            : base(GetVertexSource(), GetPixelSource(), "Polar")
        {

        }

        public void Run(UploadBuffer<ViewBufferData> buffer, Matrix transform, Vector4 crop, float multiplier, float farplane, ShaderResourceView texture, SamplerState sampler)
        {
            buffer.SetData(new ViewBufferData
            {
                Transform = transform,
                Crop = crop,
                Multiplier = multiplier,
                Farplane = farplane
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, buffer.Handle);
            dev.Pixel.SetConstantBuffer(0 , buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetSampler(0, sampler);

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            UnbindShader(dev);
        }

        private static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;  
}};

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float4 crop;
    float multiplier;
    float farplane;
}};

VertexOut main(uint id: SV_VertexID) {{
    VertexOut o;
    float2 canonical = float2((id << 1) & 2, id & 2);
    canonical = canonical * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);
    o.projPos = mul(transform, o.projPos);

    o.rayDir = normalize(mul(transform, float4(canonical.xy, farplane, 0.0)).xyz);    
    o.rayDir.y *= -1.0;

    return o;
}};

";
        }

        private static string GetPixelSource()
        {
            return $@"
Texture2D<float4> tex : register(t0);

SamplerState texSampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float4 crop;
    float multiplier;
    float farplane;
}};

{Utility.ToSrgbFunction()}

struct PixelIn {{
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;  
}};

float4 main(PixelIn i) : SV_TARGET {{
    const float PI = 3.14159265358979323846264;
    float2 polarDirection;
    float3 normalizedDirection = normalize(i.rayDir);
    // t computation
    polarDirection.y = acos(normalizedDirection.y) / PI;
    // s computation
    polarDirection.x = normalizedDirection.x == 0 ? 
        PI / 2.0 * sign(normalizedDirection.z) :
        atan2(normalizedDirection.z, normalizedDirection.x);
    polarDirection.x = polarDirection.x / (2.0 * PI) + 0.25;
    if(polarDirection.x < 0.0) polarDirection.x += 1.0;
    
    float4 color = tex.Sample(texSampler, polarDirection);
    color.rgb *= multiplier;
    {ApplyColorCrop("polarDirection")}
    return toSrgb(color);
}}
";
        }
    }
}
