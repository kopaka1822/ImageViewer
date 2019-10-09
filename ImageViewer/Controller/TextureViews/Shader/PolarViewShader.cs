using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class PolarViewShader : ViewShader
    {
        public PolarViewShader() 
            : base(GetVertexSource(), GetPixelSource(), "Polar")
        {

        }

        public static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;  
}};

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    int4 crop;
    float multiplier;
    float farplane;
}};

VertexOut main(uint id: SV_VertexID) {{
    VertexOut o;
    float canonical = float2((id << 1) & 2, id & 2);
    canonical = texcoord * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);
    o.projPos = transform * o.projPos;

    o.rayDir = normalize((transform * float4(canonical.xy, farplane, 0.0)).xyz);    
    o.rayDir.y *= -1.0;

    return o;
}};

";
        }

        public static string GetPixelSource()
        {
            return $@"
Texture2D<float> tex : register(t0);

SamplerState sampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    int4 crop;
    float multiplier;
    float farplane;
}};

{Utility.ToSrgbFunction()}

float4 main(float3 raydir : RAYDIR) : SV_TARGET {{
    const float PI = 3.14159265358979323846264;
    float2 polarDirection;
    float3 normalizedDirection = normalize(raydir);
    // t computation
    polarDirection.y = acos(normalizedDirection.y) / PI;
    // s computation
    polarDirection.x = normalizedDirection.x == 0 ? 
        PI / 2.0 * sign(normalizedDirection.z) :
        atan2(normalizedDirection.z, normalizedDirection.x);
    polarDirection.x = polarDirection.x / (2.0 * PI) + 0.25;
    if(polarDirection.x < 0.0) polarDirection.x += 1.0;
    
    float4 color = tex.Sample(sampler, polarDirection);
    {ApplyColorCrop("polarDirection")}
    return toSrgb(color);
}}
";
        }
    }
}
