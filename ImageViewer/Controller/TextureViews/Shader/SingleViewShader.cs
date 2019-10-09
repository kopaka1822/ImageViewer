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
    public class SingleViewShader : ViewShader
    {

        public SingleViewShader()
        : base(GetVertexSource(), GetPixelSource(), "Single")
        {
            
        }

        public void Run(UploadBuffer<ViewBufferData> buffer, Matrix transform, Vector4 crop, float multiplier, ShaderResourceView texture, SamplerState sampler)
        {
            buffer.SetData(new ViewBufferData
            {
                Transform = transform,
                Crop = crop,
                Multiplier = multiplier
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
    o.projPos = mul(transform, o.projPos);
    return o;
}};
";
        }

        public static string GetPixelSource()
        {
            return $@"
Texture2D<float4> tex : register(t0);

SamplerState texSampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float4 crop;
    float multiplier;
}};

{Utility.ToSrgbFunction()}

struct PixelIn {{
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD;  
}};

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = tex.Sample(texSampler, i.texcoord);
    color.rgb *= multiplier;
    {ApplyColorCrop("i.texcoord")}
    return toSrgb(color);
}}
";
        }
    }
}
