using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class SingleViewShader : ViewShader
    {

        public SingleViewShader(IShaderBuilder builder)
        : base(GetVertexSource(), GetPixelSource(builder), "Single")
        {
            
        }

        public void Run(UploadBuffer buffer, Matrix transform, Vector4 crop, float multiplier, bool useAbs, 
            ShaderResourceView texture, SamplerState sampler, int xaxis = 0, int yaxis = 1, int zvalue = 0)
        {
            buffer.SetData(new ViewBufferData
            {
                Transform = transform,
                Crop = crop,
                Multiplier = multiplier,
                UseAbs = useAbs?1:0,
                XAxis = xaxis,
                YAxis = yaxis,
                ZValue = zvalue
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
    o.texcoord = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    o.projPos = float4(o.texcoord * float2(2, -2) + float2(-1, 1), 0, 1);
    o.projPos = mul(transform, o.projPos);
    return o;
}};
";
        }

        public static string GetPixelSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} tex : register(t0);

SamplerState texSampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float4 crop;
    float multiplier;
    float farplane;
    bool useAbs;
    int xaxis;
    int yaxis;
    int zvalue;
}};

{Utility.ToSrgbFunction()}

struct PixelIn {{
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD;  
}};

#if {builder.Is3DInt}
float3 getCoord(float2 coord){{
    float res[3];
    uint size[3];
    tex.GetDimensions(size[0], size[1], size[2]);
    res[xaxis] = coord.x;
    res[yaxis] = coord.y;
    // convert zvalue to float
    res[3 - xaxis - yaxis] = (zvalue + 0.5f) / size[3 - xaxis - yaxis];
    return float3(res[0], res[1], res[2]);
}}
#else
float2 getCoord(float2 coord) {{ return coord; }}
#endif

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = tex.Sample(texSampler, getCoord(i.texcoord));
    color.rgb *= multiplier;
#if {1-builder.Is3DInt}
    {ApplyColorCrop("i.texcoord")}
#endif    
    {ApplyColorTransform()}
    return color;
}}
";
        }
    }
}
