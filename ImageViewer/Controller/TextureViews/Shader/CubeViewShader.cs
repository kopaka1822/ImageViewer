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
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class CubeViewShader : ViewShader
    {
        public CubeViewShader(ModelsEx models) 
            : base(models, GetVertexSource(), GetPixelSource(), "Cube")
        {

        }

        struct BufferData
        {
            public CommonBufferData Common;
            public Matrix Transform;
            public float Farplane;
        }

        public void Run(Matrix transform, float farplane, ShaderResourceView texture)
        {
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Common = GetCommonData(),
                Transform = transform,
                Farplane = farplane,
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, v.Buffer.Handle);

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
    float farplane;
}};
";
        }

        private static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 viewDir : VIEWDIR;  
}};

{ConstantBuffer()}

VertexOut main(uint id: SV_VertexID) {{
    VertexOut o;
    float2 canonical = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    canonical = canonical * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);

    o.viewDir = normalize(mul(transform, float4(canonical.xy, farplane, 0.0)).xyz);    

    return o;
}};

";
        }

        private static string GetPixelSource()
        {
            return $@"
TextureCube<float4> tex : register(t0);

SamplerState texSampler : register(s0);

{ConstantBuffer()}

{Utility.ToSrgbFunction()}

struct PixelIn {{
    float4 projPos : SV_POSITION;
    float3 viewDir : VIEWDIR;  
}};

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = tex.Sample(texSampler, i.viewDir);
    color.rgb *= multiplier;
    // TODO cropping?
    {ApplyColorTransform()}
    return color;
}}
";
        }
    }
}
