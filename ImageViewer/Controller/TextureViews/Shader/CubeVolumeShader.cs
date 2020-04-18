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
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class CubeVolumeShader : VolumeShader
    {
        struct BufferData
        {
            public CommonBufferData Common;
            public int UseFlatShading;
        }

        private readonly SamplerState sampler;

        public CubeVolumeShader(ModelsEx models) 
            : base(models, GetPixelSource(), "CubeVolumeShader")
        {
            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                Filter = Filter.MinMagMipPoint,
                BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 0.0f),
            });
        }

        public override void Dispose()
        {
            sampler?.Dispose();
            base.Dispose();
        }


        public void Run(Matrix transform, float screenAspect,
            bool useFlatShading,ShaderResourceView texture, ShaderResourceView emptySpaceTex)
        {
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Common = GetCommonData(transform, screenAspect),
                UseFlatShading = useFlatShading ? 1 : 0
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, v.Buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetShaderResource(1, emptySpaceTex);
            dev.Pixel.SetSampler(0, sampler);

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
            UnbindShader(dev);
        }

        private static string GetPixelSource()
        {
            return $@"
Texture3D<float4> tex : register(t0);
Texture3D<uint> emptySpaceTex : register(t1);
SamplerState texSampler : register(s0);

cbuffer InfoBuffer : register(b0) {{
    {CommonShaderBufferData()}
    bool useFlatShading;
}};

{Utility.ToSrgbFunction()}

{PixelInStruct()}

{CommonShaderFunctions()}

float getFlatShading(float factor) {{
    return 0.7 + 0.3 * factor;
}}

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = 0.0;
    color.a = 1.0; // transmittance    

    float3 ray = normalize(i.rayDir);    

    float3 pos;    
    if(!getIntersection(origin, ray, pos)) return color;
    
    int3 dirSign; 
    dirSign.x = ray.x < 0.0f ? -1 : 1;
    dirSign.y = ray.y < 0.0f ? -1 : 1;
    dirSign.z = ray.z < 0.0f ? -1 : 1;

    int3 intPos = clamp(int3(pos), cubeStart, cubeEnd - 1);
    float3 absRay = abs(ray);
    float3 projLength = 1.0 / (absRay + 0.00001);
    float3 distance = pos - intPos;

    if(dirSign.x == 1) distance.x = 1-distance.x;
    if(dirSign.y == 1) distance.y = 1-distance.y;
    if(dirSign.z == 1) distance.z = 1-distance.z;
    distance *= projLength;

    // adjust values for flat shading
    absRay.x = getFlatShading(absRay.x);
    absRay.y = getFlatShading(absRay.y);
    absRay.z = getFlatShading(absRay.z);

    float diffuse = 1;
    if(useFlatShading){{
        if(pos.x == fcubeStart.x || pos.x == fcubeEnd.x){{
         diffuse = absRay.x;
        }}
        if(pos.y == fcubeStart.y || pos.y == fcubeEnd.y){{
         diffuse = absRay.y;
        }}
        if(pos.z == fcubeStart.z || pos.z == fcubeEnd.z){{
         diffuse = absRay.z;
        }}
    }}

    [loop] do{{
        int skipValue = emptySpaceTex[intPos];

        if(skipValue == 0) {{
            // this block can be shaded
            float4 s = tex[intPos];
            if(!useFlatShading){{
               diffuse = 1;
            }}
            color.rgb += color.a * s.a * s.rgb * diffuse;
            color.a *= 1 - s.a;
        }}

        int numIterations = max(skipValue, 1);      

        while(numIterations-- != 0) {{
            if(distance.x < distance.y || distance.z < distance.y) {{
                if(distance.x < distance.z){{
                    intPos.x += dirSign.x;
                    distance.yz -= distance.x;
                    distance.x = projLength.x;
                    diffuse = absRay.x;
                }}    
                else{{
                    intPos.z += dirSign.z;
                    distance.xy -= distance.z;
                    distance.z = projLength.z;
                    diffuse = absRay.z;
                }}    
            }}
            else{{
                intPos.y += dirSign.y;
                distance.xz -= distance.y;
                distance.y = projLength.y;
                diffuse = absRay.y;
            }}    
        }}

    }} while(isInside(intPos) && color.a > 0.0);


    {ApplyColorTransform()}
    return toSrgb(color);
}}
";
        }
    }
}
