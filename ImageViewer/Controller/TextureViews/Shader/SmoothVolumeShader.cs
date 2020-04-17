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
    public class SmoothVolumeShader : ViewShader
    {
        struct BufferData
        {
            public CommonBufferData Common;
            public Matrix Transform;
            public Vector3 Origin;
            public float Aspect; // aspect ratio
        }

        private readonly SamplerState sampler;

        public SmoothVolumeShader(ModelsEx models) 
            : base(models, GetVertexSource(), GetPixelSource(), "SmoothVolumeShader")
        {
            sampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Border,
                AddressV = TextureAddressMode.Border,
                AddressW = TextureAddressMode.Border,
                //AddressU = TextureAddressMode.Clamp,
                //AddressV = TextureAddressMode.Clamp,
                //AddressW = TextureAddressMode.Clamp,
                Filter = Filter.MinMagMipLinear,
                BorderColor = new RawColor4(0.0f, 0.0f, 0.0f, 0.0f),
            });
        }

        public override void Dispose()
        {
            sampler?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform">transformation matrix from camera space to image space (pixel coordinates)</param>
        /// <param name="screenAspect">screen aspect ratio</param>
        /// <param name="texture"></param>
        /// <param name="emptySpaceTex"></param>
        public void Run(Matrix transform, float screenAspect, ShaderResourceView texture, ShaderResourceView emptySpaceTex)
        {
            var zero = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            Vector4.Transform(ref zero, ref transform, out var origin);
            
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Common = GetCommonData(null),
                Transform = transform,
                Origin = new Vector3(origin.X, origin.Y, origin.Z),
                Aspect = screenAspect
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, v.Buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetSampler(0, sampler);
            dev.Pixel.SetShaderResource(1, emptySpaceTex);

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
    matrix transform; // camera rotation
    float3 origin; // camera origin
    float aspect;
}};
";
        }

        private static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;
}};

{ConstantBuffer()}

VertexOut main(uint id : SV_VertexID) {{
    VertexOut o;
    float2 canonical = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    canonical = canonical * float2(2, -2) + float2(-1, 1);

    o.projPos = float4(canonical, 0, 1);

    o.rayDir = mul((float3x3)(transform), normalize(float3(canonical.x * aspect, -canonical.y, 1.0)));    

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
}};

bool isInside(float3 pos, float3 size) {{
    [unroll] for(int i = 0; i < 3; ++i) {{
        if(pos[i] < 0.0 || pos[i] > size[i]) return false;
    }}
    return true;
}}

bool getIntersection(float3 origin, float3 dir, float3 size, out float3 intersect) {{
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
        }} else if(origin[i] > size[i]) {{
            quadrant[i] = RIGHT;    
            candidatePlane[i] = size[i];
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
            if(intersect[i] < 0.0 || intersect[i] > size[i]) return false;
        }} else 
            intersect[i] = candidatePlane[i];
    }}

    return true;
}}

float4 main(PixelIn i) : SV_TARGET {{
    float4 color = 0.0;
    color.a = 1.0; // transmittance

    int width, height, depth;
    tex.GetDimensions(width, height, depth);
    int3 size = uint3(width, height, depth);
    float3 fsize = float3(size);    

    // the height of the cube is 2.0 in world space. Width and Depth depend on aspect of height
    const float stepsize = 4.0;
    const float invStepsize = 1.0 / stepsize;

    float3 unitRay = normalize(i.rayDir);
    float3 ray = unitRay / stepsize;

    float3 pos;    
    if(!getIntersection(origin, ray, fsize, pos)) return color;

    // convert from pixel coordinates to texel ([0, 1])
    float3 invSize = 1 / fsize;    
    
    float skipped = 0.0;
    [loop] do{{
        float3 pos2 = float3(pos.xy,1-pos.z);

        //skip empty space
        pos += max(int(emptySpaceTex[clamp(int3(pos), 0, size-1)]) - 1, 0) * unitRay;

        if(!isInside(pos, fsize)) break;

        float4 s = tex.SampleLevel(texSampler, pos * invSize, 0);

        float invAlpha = pow(max(1.0 - s.a, 0.0), invStepsize);
        float alpha = 1.0 - invAlpha;
        color.rgb += color.a * alpha * s.rgb;
        color.a *= invAlpha;
        
        pos += ray;
    }} while(color.a > 0.0);
   
    {ApplyColorTransform()}
    return toSrgb(color);
}}
";
        }
    }
}
