using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class RayMarchingShader : ViewShader
    {
        public struct ViewBufferData
        {
            public Matrix Transform;
            public Matrix WorldToImage;
            public Vector4 Crop;
            public float Multiplier;
            public float Farplane;
            public int UseAbs;
            
        }

        private readonly SamplerState sampler;

        public RayMarchingShader() 
            : base(GetVertexSource(), GetPixelSource(), "RayMarchingShader")
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


        public void Run(UploadBuffer buffer, Matrix rayTransform, Matrix worldToImage, float multiplier, float farplane,
            bool useAbs, ShaderResourceView texture)
        {
            buffer.SetData(new ViewBufferData
            {
                Transform = rayTransform,
                WorldToImage = worldToImage,
                Multiplier = multiplier,
                Farplane = farplane,
                UseAbs = useAbs ? 1 : 0
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
    float3 rayDir : RAYDIR;
    float3 origin : RAYORIGIN;
}};

{ConstantBuffer()}

VertexOut main(uint id : SV_VertexID) {{
    VertexOut o;
    float2 canonical = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    canonical = canonical * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);

    o.rayDir = normalize(mul(transform, float4(canonical.xy, farplane, 0.0)).xyz);    
    
    float4 origin = mul(toImage, float4(0, 0, 0, 1));
    o.origin = origin.xyz / origin.w;    

    return o;
}}
";
        }

        private static string GetPixelSource()
        {
            return $@"
Texture3D<float4> tex : register(t0);
SamplerState texSampler : register(s0);

{ConstantBuffer()}
{Utility.ToSrgbFunction()}

struct PixelIn {{
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;
    float3 origin : RAYORIGIN;
}};

bool isInside(float3 pos) {{
    [unroll] for(int i = 0; i < 3; ++i) {{
        if(pos[i] < 0.0 || pos[i] > 1.0) return false;
    }}
    return true;
}}

bool isInside(uint3 pos, uint3 dimension) {{
    [unroll] for(int i = 0; i < 3; ++i) {{
        if(pos[i] >= dimension[i]) return false;
    }}
    return true;
}}


bool getIntersection(float3 origin, float3 dir, out float3 intersect) {{
    intersect = origin;
    
    const int RIGHT = 0;
    const int LEFT = 1;
    const int MIDDLE = 2;
    int3 quadrant = 0;
    float3 candidatePlane = 0;
    bool inside = true;
    
    // find candidate planes
    [unroll] for(int i = 0; i < 3; ++i) {{
        if(origin[i] < 0.0) {{
            quadrant[i] = LEFT;
            candidatePlane[i] = 0.0;
            inside = false;
        }} else if(origin[i] > 1.0) {{
            quadrant[i] = RIGHT;    
            candidatePlane[i] = 1.0;
            inside = false;
        }}
        else quadrant[i] = MIDDLE;
    }}

    if(inside) return true;

    // calculate t distances to candidate planes
    float3 maxT = -1.0;
    [unroll] for(i = 0; i < 3; ++i) {{
        if(quadrant[i] != MIDDLE && dir[i] != 0.0)
            maxT[i] = (candidatePlane[i]-origin[i]) / dir[i];
    }}

    // get largest maxT for final choice of intersection
    int whichPlane = 0;
    float maxTPlane = maxT[0];
    [unroll] for(i = 1; i < 3; ++i) {{
        if(maxTPlane < maxT[i]){{
            whichPlane = i;
            maxTPlane = maxT[i];
        }}
    }}
    
    // check final candidate actually inside box
    if(maxTPlane < 0.0) return false;
    
    [unroll] for(i = 0; i < 3; ++i) {{
        if(i != whichPlane) {{
            intersect[i] =  origin[i] + maxTPlane * dir[i];
            if(intersect[i] < 0.0 || intersect[i] > 1.0) return false;
        }} else 
            intersect[i] = candidatePlane[i];
    }}

    return true;
}}

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = 0.0;
    color.a = 1.0; // transmittance

    uint width, height, depth;
    tex.GetDimensions(width, height, depth);
    uint3 textureDimension = uint3(width,height, depth);
    
    // transform ray to image space
    float3 ray = i.rayDir;
    ray = mul(toImage, float4(ray, 0.0)).xyz;
    ray = normalize(ray);
    float3 pos;    

    if(!getIntersection(i.origin, ray, pos)) return color;
    
    int3 dirSign; 
    dirSign.x = ray.x < 0.0f ? -1 : 1;
    dirSign.y = ray.y < 0.0f ? -1 : 1;
    dirSign.z = ray.z < 0.0f ? -1 : 1;


    float3 voxelPos = pos * (float3(textureDimension) - 0.00001);
    uint3 rayPos = uint3(voxelPos); 
    float3 absDir = abs(ray);
    float3 projLength = 1.0 / (absDir + 0.00001);
    float3 distance = voxelPos - rayPos;
    if(dirSign.x == 1) distance.x = 1-distance.x;
    if(dirSign.y == 1) distance.y = 1-distance.y;
    if(dirSign.z == 1) distance.z = 1-distance.z;
    distance *= projLength;

    float4 first = tex[rayPos];
    color.rgb += color.a * first.a * first.rgb;
    color.a *= 1 - first.a;
     

    [loop] do{{
        if(distance.x < distance.y || distance.z < distance.y) {{
            if(distance.x < distance.z){{
                rayPos.x += dirSign.x;
                distance.yz -= distance.x;
                distance.x = projLength.x;
            }}    
            else{{
                rayPos.z += dirSign.z;
                distance.xy -= distance.z;
                distance.z = projLength.z;
            }}    
        }}
        else{{
            rayPos.y += dirSign.y;
            distance.xz -= distance.y;
            distance.y = projLength.y;
        }}    
    

        float4 s = tex[rayPos];
        color.rgb += color.a * s.a * s.rgb;
        color.a *= 1 - s.a;
        
    }} while(isInside(rayPos,textureDimension) && color.a > 0.0);
   
    color.rgb *= multiplier;
    {ApplyColorTransform()}
    return color;
}}
";
        }

        private static string ConstantBuffer()
        {
            return @"
cbuffer InfoBuffer : register(b0) {
    matrix transform;
    matrix toImage;
    float4 crop;
    float multiplier;
    float farplane;
    bool useAbs;
};
";
        }
    }
}
