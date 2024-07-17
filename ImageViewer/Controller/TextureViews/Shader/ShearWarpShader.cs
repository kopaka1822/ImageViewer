using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class ShearWarpShader : ViewShader
    {
        public struct ViewBufferData
        {
            public Matrix Transform;

            public Vector4 Crop;

            public float Multiplier;
            public int UseAbs;
            public int FixedAxis;
            public int FixedAxisDim;

            public Vector2 Aspects;
        }

        public ShearWarpShader(ModelsEx models) :
            base(models, GetVertexSource(), GetPixelSource(), "ShearWarpShader")
        {

        }

        // TODO refactor this
        public void Run(UploadBuffer buffer, Matrix projection, Matrix model, float multiplier,
            bool useAbs, ShaderResourceView texture, SamplerState sampler, Size3 imgSize)
        {
            // determine which axis should be used for slices
            var zDir = new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
            Vector4.Transform(ref zDir, ref model, out var outDir);
            int fixedAxis = 0;
            for (int i = 1; i < 3; ++i)
            {
                if (Math.Abs(outDir[i]) > Math.Abs(outDir[fixedAxis]))
                    fixedAxis = i;
            }

            buffer.SetData(new ViewBufferData
            {
                Transform = model * projection,
                UseAbs = useAbs?1:0,
                Multiplier = multiplier,
                FixedAxis = fixedAxis,
                FixedAxisDim = imgSize[fixedAxis],
                Aspects = new Vector2(imgSize.X / (float)imgSize.Y, imgSize.Z / (float)imgSize.Y)
            });

            // bind resources
            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetSampler(0, sampler);

            dev.DrawQuad(imgSize[fixedAxis]);

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            UnbindShader(dev);
        }

        private static string GetVertexSource()
        {
            return $@"
{ConstantBuffer()}

struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 texcoord : TEXCOORD;
}};

VertexOut main(uint id : SV_VertexID) {{
    uint vid = id & 3; // lowest two bits for vertex span
    uint sliceId = id >> 2;

    // unormed in range [0, 1]
    // canonical in range [-1, 1]
    float2 unormed = float2(((vid << 1) & 2) / 2, (vid & 2) / 2);
    float2 canonical = unormed * float2(2, -2) + float2(-1, 1);
    
    // configure model space coord and texture coordinate
    float3 coord = float3(aspects.x, 1.0, aspects.y);
    float3 texcoord = (sliceId + 0.5) / fixedAxisDim; // texture coordinate for fixed axis

    if(fixedAxis == 0) {{
        // span yz
        coord.yz *= canonical;
        texcoord.yz = 1.0-unormed;
        coord.x = texcoord.x * 2.0 - aspects.x;
    }} else if(fixedAxis == 1) {{
        // span xz
        coord.xz *= canonical;
        texcoord.xz = unormed;
        coord.y = texcoord.y * -2.0 + 1.0;
    }} else {{
        // span xy
        coord.xy *= canonical;
        texcoord.xy = unormed;
        coord.z = texcoord.z * 2.0 - aspects.y;
    }}

    // transform into projection space
    VertexOut o;
    o.projPos = mul(transform, float4(coord, 1.0));
    o.texcoord = texcoord;

    return o;
}}
";
        }

        private static string GetPixelSource()
        {
            return @"
Texture3D<float4> tex : register(t0);
SamplerState texSampler : register(s0);

struct PixelIn {
    float4 projPos : SV_POSITION;
    float3 texcoord : TEXCOORD;
};

float4 main(PixelIn i) : SV_TARGET {
    return tex.SampleLevel(texSampler, i.texcoord, 0);
}
";
        }

        private static string ConstantBuffer()
        {
            return @"
cbuffer InfoBuffer : register(b0) {
    matrix transform;
    float4 crop;
    float multiplier;
    bool useAbs;
    int fixedAxis;
    int fixedAxisDim;
    float2 aspects;
};
";
        }
    }
}
