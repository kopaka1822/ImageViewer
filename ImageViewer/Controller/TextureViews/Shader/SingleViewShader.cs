using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class SingleViewShader : ViewShader
    {

        public SingleViewShader()
        : base(GetVertexSource(), GetPixelSource(), "Single")
        {
            
        }

        public static string GetVertexSource()
        {
            return $@"

struct VertexOut {{
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD;  
}};

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
}};

VertexOut main(uint id: SV_VertexID) {{
    VertexOut o;
    o.texcoord = float2((id << 1) & 2, id & 2);
    o.projPos = float4(o.texcoord * float2(2, -2) + float2(-1, 1), 0, 1);
    o.projPos = transform * o.projPos;
    return o;
}};
";
        }

        public static string GetPixelSource()
        {
            return $@"
Texture2D<float4> tex : register(t0);

SamplerState sampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    int4 crop;
    float multiplier;
}};

{Utility.ToSrgbFunction()}

float4 main(float2 texcoord : TEXCOORD) : SV_TARGET {{
    float4 color = tex.Sample(sampler, texcoord);
    color.rgb *= multiplier;
    {ApplyColorCrop("texcoord")}
    return toSrgb(color);
}}
";
        }
    }
}
