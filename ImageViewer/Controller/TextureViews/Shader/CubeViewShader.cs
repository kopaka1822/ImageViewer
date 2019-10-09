using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class CubeViewShader : ViewShader
    {
        public CubeViewShader() 
            : base(GetVertexSource(), GetPixelSource(), "Cube")
        {

        }

        public static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 viewDir : VIEWDIR;  
}};

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float4 crop;
    float multiplier;
    float farplane;
}};

VertexOut main(uint id: SV_VertexID) {{
    VertexOut o;
    float canonical = float2((id << 1) & 2, id & 2);
    canonical = texcoord * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);
    o.projPos = transform * o.projPos;

    o.viewDir = normalize((transform * float4(canonical.xy, farplane, 0.0)).xyz);    

    return o;
}};

";
        }

        public static string GetPixelSource()
        {
            return $@"
TextureCube<float4> tex : register(t0);

SamplerState sampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float4 crop;
    float multiplier;
    float farplane;
}};

{Utility.ToSrgbFunction()}

float4 main(float3 viewDir : VIEWDIR) : SV_TARGET {{
    float4 color = tex.Sample(sampler, viewDir);
    color.rgb *= multiplier;
    // TODO cropping?
    return toSrgb(color);
}}
";
        }
    }
}
