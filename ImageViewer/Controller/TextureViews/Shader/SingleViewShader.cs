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
        }

        public void Run(Matrix transform, ShaderResourceView texture, ShaderResourceView overlay, int xaxis, int yaxis, int zvalue)
        {
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Common = GetCommonData(overlay),
                Transform = transform,
                XAxis = xaxis,
                YAxis = yaxis,
                ZValue = zvalue
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0 , v.Buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetShaderResource(1, overlay);
            dev.Pixel.SetSampler(0, v.GetSampler());

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
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
{builder.SrvSingleType} overlay : register(t1);

SamplerState texSampler : register(s0);

{ConstantBuffer()}

{Utility.ToSrgbFunction()}

struct PixelIn {{
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD;  
}};

#if {builder.Is3DInt}
float3 getCoord(float2 coord, out float2 ddxystep){{
    float res[3];
    uint size[3];
    tex.GetDimensions(size[0], size[1], size[2]);
    res[xaxis] = coord.x;
    res[yaxis] = coord.y;
    // convert zvalue to float
    res[3 - xaxis - yaxis] = (zvalue + 0.5f) / size[3 - xaxis - yaxis];
    ddxystep.x = 1.0 / size[xaxis];
    ddxystep.y = 1.0 / size[yaxis];
    return float3(res[0], res[1], res[2]);
}}
#define TexcoordT float3
#else
float2 getCoord(float2 coord, out float2 ddxystep) {{ 
    uint size[2];
    tex.GetDimensions(size[0], size[1]);
    ddxystep.x = 1.0 / size[0];
    ddxystep.y = 1.0 / size[1];
    return coord; 
}}
#define TexcoordT float2
#endif


#define N_SAMPLES 16
/*static const float2 samplePoints[N_SAMPLES] = {{
    float2(-3.0/8.0,  1.0/8.0),
    float2(-1.0/8.0, -3.0/8.0),
    float2( 1.0/8.0,  3.0/8.0),
    float2( 3.0/8.0, -1.0/8.0)
}};*/

static const float2 samplePoints[N_SAMPLES] = {{
    float2( 1.0/16.0,  1.0/16.0),
    float2(-1.0/16.0, -3.0/16.0),
    float2(-3.0/16.0,  2.0/16.0),
    float2( 4.0/16.0, -1.0/16.0),
    float2(-5.0/16.0, -2.0/16.0),
    float2( 2.0/16.0,  5.0/16.0),
    float2( 5.0/16.0,  3.0/16.0),
    float2( 3.0/16.0, -5.0/16.0),
    float2(-2.0/16.0,  6.0/16.0),
    float2( 0.0/16.0, -7.0/16.0),
    float2(-4.0/16.0, -6.0/16.0),
    float2(-6.0/16.0,  4.0/16.0),
    float2(-8.0/16.0,  0.0/16.0),
    float2( 7.0/16.0, -4.0/16.0),
    float2( 6.0/16.0,  7.0/16.0),
    float2(-7.0/16.0, -8.0/16.0)
}};

float4 getColor(TexcoordT coord)
{{
    float4 color = tex.Sample(texSampler, coord);
    {ApplyColorTransform()}
    {(builder.Is3D ? ApplyOverlay3D("coord", "color") : ApplyOverlay2D("coord", "color"))}
    return color;
}}

float4 main(PixelIn i) : SV_TARGET {{
    float2 maxdxy;
    TexcoordT texcoord = getCoord(i.texcoord, maxdxy);
    
    TexcoordT dx = ddx(texcoord);
    TexcoordT dy = ddy(texcoord);
    float4 color = 0.0;

    if(length(dx) > maxdxy.x * 1.01 || length(dy) > maxdxy.y * 1.01) {{
        // use supersampling for minification
        [unroll] for(uint si = 0; si < N_SAMPLES; ++si)
            color += getColor(texcoord + samplePoints[si].x * dx + samplePoints[si].y * dy);
        
        color /= N_SAMPLES;
    }} else color = getColor(texcoord);

    return toSrgb(color);
}}
";
        }
    }
}
