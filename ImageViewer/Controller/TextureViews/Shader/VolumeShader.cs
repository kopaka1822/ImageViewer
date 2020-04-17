using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;

namespace ImageViewer.Controller.TextureViews.Shader
{
    // base class for the volume shader
    public class VolumeShader : ViewShader
    {
        protected new struct CommonBufferData
        {
            public ViewShader.CommonBufferData Common;
            public Matrix Transform; // camera to image transform
            public Vector3 Origin; // ray origin in image space
            public float Aspect; // aspect ratio
        }

        protected VolumeShader(ModelsEx models, string pixelSource, string debugName)
        : base(models, GetVertexSource(), pixelSource, debugName)
        {}

        protected CommonBufferData GetCommonData(Matrix transform, float screenAspect)
        {
            var zero = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            Vector4.Transform(ref zero, ref transform, out var origin);

            return new CommonBufferData
            {
                Common = GetCommonData(null),
                Transform = transform,
                Aspect = screenAspect,
                Origin = new Vector3(origin.X, origin.Y, origin.Z)
            };
        }

        protected new static string CommonShaderBufferData()
        {
            return ViewShader.CommonShaderBufferData() + @"
matrix transform; // camera rotation
float3 origin; // camera origin
float aspect;
    ";
        }

        // vertex shader is the same for all volumes
        private static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;
}};

cbuffer InfoBuffer : register(b0) {{
    {CommonShaderBufferData()}
}};

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

        protected static string PixelInStruct()
        {
            return @"
struct PixelIn {
    float4 projPos : SV_POSITION;
    float3 rayDir : RAYDIR;
};
";
        }

        protected static string CommonShaderFunctions()
        {
            return @"
bool isInside(float3 pos, float3 size) {
    [unroll] for(int i = 0; i < 3; ++i) {
        if(pos[i] < 0.0 || pos[i] > size[i]) return false;
    }
    return true;
}

bool isInside(int3 pos, int3 size) {
    [unroll] for(int i = 0; i < 3; ++i) {
        if(pos[i] < 0 || pos[i] >= size[i]) return false;
    }
    return true;
}

bool getIntersection(float3 origin, float3 dir, float3 size, out float3 intersect) {
    intersect = origin;
    
    const int RIGHT = 0;
    const int LEFT = 1;
    const int MIDDLE = 2;
    int3 quadrant = 0;
    float3 candidatePlane = 0;
    bool inside = true;
    
    // find candidate planes
    [unroll] for(int i = 0; i < 3; ++i) {
        if(origin[i] < 0.0) {
            quadrant[i] = LEFT;
            candidatePlane[i] = 0.0;
            inside = false;
        } else if(origin[i] > size[i]) {
            quadrant[i] = RIGHT;    
            candidatePlane[i] = size[i];
            inside = false;
        }
        else quadrant[i] = MIDDLE;
    }

    if(inside) return true;

    // calculate t distances to candidate planes
    float3 maxT = -1.0;
    [unroll] for(i = 0; i < 3; ++i) {
        if(quadrant[i] != MIDDLE && dir[i] != 0.0)
            maxT[i] = (candidatePlane[i]-origin[i]) / dir[i];
    }

    // get largest maxT for final choice of intersection
    int whichPlane = 0;
    float maxTPlane = maxT[0];
    [unroll] for(i = 1; i < 3; ++i) {
        if(maxTPlane < maxT[i]){
            whichPlane = i;
            maxTPlane = maxT[i];
        }
    }
    
    // check final candidate actually inside box
    if(maxTPlane < 0.0) return false;
    
    [unroll] for(i = 0; i < 3; ++i) {
        if(i != whichPlane) {
            intersect[i] =  origin[i] + maxTPlane * dir[i];
            if(intersect[i] < 0.0 || intersect[i] > size[i]) return false;
        } else 
            intersect[i] = candidatePlane[i];
    }

    return true;
}
";
        }
    }
}
