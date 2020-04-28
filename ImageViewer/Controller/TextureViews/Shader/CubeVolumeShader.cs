using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private struct BufferData
        {
            public CommonBufferData Common;
            public int UseFlatShading;
        }

        private struct CoordBufferData
        {
            public CommonBufferData Common;
            public Float3 Ray;
        }

        private readonly ImageFramework.DirectX.Shader coordShader;
        private readonly GpuBuffer coordDstBuffer;

        public CubeVolumeShader(ModelsEx models)
            : base(models, GetPixelSource(), "CubeVolumeShader")
        {
            coordDstBuffer = new GpuBuffer(Marshal.SizeOf(typeof(Size3)), 1);
            coordShader = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Compute, GetComputeSource(), "CubeCoordShader");
        }

        public override void Dispose()
        {
            coordDstBuffer.Dispose();
            coordShader.Dispose();
            base.Dispose();
        }

        public void Run(Matrix transform, bool useFlatShading, ShaderResourceView texture, ShaderResourceView emptySpaceTex)
        {
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Common = GetCommonData(transform, models.Display.ClientAspectRatioScalar),
                UseFlatShading = useFlatShading ? 1 : 0
            });

            var dev = Device.Get();
            BindShader(dev);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, v.Buffer.Handle);

            dev.Pixel.SetShaderResource(0, texture);
            dev.Pixel.SetShaderResource(1, emptySpaceTex);

            dev.DrawQuad();

            // unbind
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
            UnbindShader(dev);
        }

        public Size3 GetIntersection(Matrix transform, Vector2 mousePos, ShaderResourceView emptySpaceTex)
        {
            // transform mousePos to ray direction
            var v = models.ViewData;
            v.Buffer.SetData(new CoordBufferData
            {
                Common = GetCommonData(transform, models.Display.ClientAspectRatioScalar),
                Ray = GetRayDirFromCanonical(transform, mousePos),
            });

            var dev = Device.Get();
            dev.Compute.Set(coordShader.Compute);
            dev.Compute.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Compute.SetShaderResource(0, emptySpaceTex);
            dev.Compute.SetUnorderedAccessView(0, coordDstBuffer.View);

            dev.Dispatch(1, 1);

            // unbind
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);

            // obtain data
            var read = models.SharedModel.Download;
            read.CopyFrom(coordDstBuffer, Marshal.SizeOf(typeof(Size3)));
            return read.GetData<Size3>();
        }

        private static string GetPixelSource()
        {
            return $@"
Texture3D<float4> tex : register(t0);
Texture3D<uint> emptySpaceTex : register(t1);

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
    // color
    float3 c = 0.0;
    // transmittance
    float3 t = 1.0;

    float3 ray = normalize(i.rayDir);    

    float3 pos;    
    if(!getIntersection(origin, ray, pos)) return float4(0, 0, 0, 1);
    
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
            c += calcColor(s, t) * diffuse;
            t *= getTransmittance(s);
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

    }} while(isInside(intPos) && dot(t, 1) > 0.01);

    float4 color = float4(c, dot(t, 1.0 / 3.0));
    {ApplyColorTransform()}
    return toSrgb(color);
}}
";
        }

        // determines the position of the first intersection
        private string GetComputeSource()
        {
            return $@"
Texture3D<uint> emptySpaceTex : register(t0);
RWStructuredBuffer<int3> out_buffer : register(u0);

cbuffer InfoBuffer : register(b0) {{
    {CommonShaderBufferData()}
    float3 ray;
}};

{CommonShaderFunctions()}

[numthreads(1, 1, 1)]
void main(){{
    float3 pos;    
    out_buffer[0] = -1;
    if(!getIntersection(origin, ray, pos)) {{
        // no intersection at all!    
        return;
    }}

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

    [loop] do{{
        int skipValue = emptySpaceTex[intPos];

        if(skipValue == 0) {{
            // this block can be shaded
            out_buffer[0] = intPos; // finished!
            return;
        }}

        int numIterations = max(skipValue, 1);      

        while(numIterations-- != 0) {{
            if(distance.x < distance.y || distance.z < distance.y) {{
                if(distance.x < distance.z){{
                    intPos.x += dirSign.x;
                    distance.yz -= distance.x;
                    distance.x = projLength.x;
                }}    
                else{{
                    intPos.z += dirSign.z;
                    distance.xy -= distance.z;
                    distance.z = projLength.z;
                }}    
            }}
            else{{
                intPos.y += dirSign.y;
                distance.xz -= distance.y;
                distance.y = projLength.y;
            }}    
        }}

    }} while(isInside(intPos));
    // no intersection along the ray...
}}
";
        }
    }
}
