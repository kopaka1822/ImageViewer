using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class SingleViewShader : ViewShader
    {

        public SingleViewShader(ModelsEx models, IShaderBuilder builder)
        : base(models, GetVertexSource(), GetPixelSource(builder), "Single")
        {
            
        }

        struct BufferData
        {
            public CommonBufferData Common;
            public Matrix Transform;
            public int XAxis;
            public int YAxis;
            public int ZValue;
            public int Layer;
        }

        public void Run(Matrix transform, ShaderResourceView texture, int layer, int xaxis, int yaxis, int zvalue)
        {
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Common = GetCommonData(),
                Transform = transform,
                XAxis = xaxis,
                YAxis = yaxis,
                ZValue = zvalue,
                Layer = layer
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0 , v.Buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetSampler(0, v.GetSampler());

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            UnbindShader(dev);
        }

        private static string ConstantBuffer()
        {
            return $@"
cbuffer InfoBuffer : register(b0) {{
    {CommonShaderBufferData()}
    matrix transform;
    int xaxis;
    int yaxis;
    int zvalue;
    int layer;
}};
";
        }

        public static string GetVertexSource()
        {
            return $@"

struct VertexOut {{
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD;  
}};

{ConstantBuffer()}

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

{ConstantBuffer()}

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
#define TexcoordT float3
#else
float2 getCoord(float2 coord) {{ return coord; }}
#define TexcoordT float2
#endif

float4 main(PixelIn i) : SV_TARGET {{
    TexcoordT texcoord = getCoord(i.texcoord);
    float4 color = tex.Sample(texSampler, texcoord);
    color.rgb *= multiplier;
    {ApplyColorCrop("texcoord", "layer", builder.Is3D)}
    {ApplyColorTransform()}
    return color;
}}
";
        }
    }
}
