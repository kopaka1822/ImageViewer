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
    public class RayCastingShader : ViewShader
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

        public RayCastingShader() 
            : base(GetVertexSource(), GetPixelSource(), "RayCastingShader")
        {
            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                Filter = Filter.MinMagMipLinear,
                BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 0.0f),
            });
        }

        public override void Dispose()
        {
            sampler?.Dispose();
            base.Dispose();
        }


        public void Run(UploadBuffer buffer, Matrix rayTransform, Matrix worldToImage, float multiplier, float farplane,
            bool useAbs, ShaderResourceView texture, ShaderResourceView emptySpaceTex)
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
            dev.Pixel.SetShaderResource(1, emptySpaceTex);

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
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
Texture3D<uint> emptySpaceTex : register(t1);
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

    // the height of the cube is 2.0 in world space. Width and Depth depend on aspect of height
    const float stepsize = 4.0;
    float3 ray = normalize(i.rayDir) * 2.0f / float(height) / stepsize;

    // transform ray to image space
    ray = mul(toImage, float4(ray, 0.0)).xyz;
    float3 pos;    

    if(!getIntersection(i.origin, ray, pos)) return color;
    
    [loop] do{{
        float4 s = tex.SampleLevel(texSampler, pos, 0);
        float invAlpha = pow(max(1.0 - s.a, 0.0), 1.0 / stepsize);
        float alpha = 1.0 - invAlpha;
        color.rgb += color.a * alpha * s.rgb;
        color.a *= invAlpha;
        
        pos += ray;

        //skip empty space
        pos += emptySpaceTex[pos] * ray;

    }} while(isInside(pos) && color.a > 0.0);
   
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
