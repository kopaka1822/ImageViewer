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

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = 0.0;
    color.a = 1.0; // transmittance    

    uint width, height, depth;
    tex.GetDimensions(width, height, depth);
    int3 size = uint3(width,height, depth);
    float3 fsize = float3(size);    

    // transform ray to image space
    float3 ray = normalize(i.rayDir);    

    float3 pos;    
    if(!getIntersection(origin, ray, fsize, pos)) return color;
    
    int3 dirSign; 
    dirSign.x = ray.x < 0.0f ? -1 : 1;
    dirSign.y = ray.y < 0.0f ? -1 : 1;
    dirSign.z = ray.z < 0.0f ? -1 : 1;

    int3 intPos = clamp(int3(pos), 0, size - 1);
    float3 absRay = abs(ray);
    float3 projLength = 1.0 / (absRay + 0.00001);
    float3 distance = pos - intPos;

    if(dirSign.x == 1) distance.x = 1-distance.x;
    if(dirSign.y == 1) distance.y = 1-distance.y;
    if(dirSign.z == 1) distance.z = 1-distance.z;
    distance *= projLength;

    //float3 voxelPos = pos * (float3(textureDimension) - 0.00001);
    //uint3 rayPos = uint3(voxelPos); 
    //float3 absDir = abs(ray);
    //float3 projLength = 1.0 / (absDir + 0.00001);
    //float3 distance = voxelPos - rayPos;

    //dot(normal,absDir) for first intersection with bounding box
    float diffuse = 1;
    if(useFlatShading){{
        if(pos.x == fsize.x || pos.x == 0){{
         diffuse = absRay.x;
        }}
        if(pos.y == fsize.y || pos.y == 0){{
         diffuse = absRay.y;
        }}
        if(pos.z == fsize.z || pos.z == 0){{
         diffuse = absRay.z;
        }}
    }}

    float skipped = 0.0;
    [loop] do{{

        float4 s = tex[intPos];
        if(!useFlatShading){{
           diffuse = 1;
        }}
        color.rgb += color.a * s.a * s.rgb * diffuse;
        color.a *= 1 - s.a;

        int numIterations = max(emptySpaceTex[intPos], 1);
        skipped += numIterations - 1;        

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

    }} while(isInside(intPos, size) && color.a > 0.0);


    {ApplyColorTransform()}
    color.r += skipped;
    return toSrgb(color);
}}
";
        }
    }
}
