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
    public class CubeViewShader : ViewShader
    {
        public CubeViewShader() 
            : base(GetVertexSource(), GetPixelSource(), "Cube")
        {

        }

        public void Run(UploadBuffer<ViewBufferData> buffer, Matrix transform, float multiplier, float farplane, ShaderResourceView texture, SamplerState sampler)
        {
            buffer.SetData(new ViewBufferData
            {
                Transform = transform,
                Multiplier = multiplier,
                Farplane = farplane
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, buffer.Handle);

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
    float2 canonical = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    canonical = canonical * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);

    o.viewDir = normalize((transform * float4(canonical.xy, farplane, 0.0)).xyz);    

    return o;
}};

";
        }

        private static string GetPixelSource()
        {
            return $@"
TextureCube<float4> tex : register(t0);

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
    float3 viewDir : VIEWDIR;  
}};

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = tex.Sample(texSampler, i.viewDir);
    color.rgb *= multiplier;
    // TODO cropping?
    return toSrgb(color);
}}
";
        }
    }
}
