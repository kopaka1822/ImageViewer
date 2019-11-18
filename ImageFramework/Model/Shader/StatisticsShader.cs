using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Utility;

namespace ImageFramework.Model.Shader
{
    public class StatisticsShader : IDisposable
    {
        private readonly DirectX.Shader shader;
        private readonly DirectX.Shader shader3d;
        private readonly int LocalSizeX = 128;
        private readonly int LocalSizeY = 4;
        private readonly UploadBuffer cbuffer;
        private readonly string returnValue;

        // predefined return values
        public static readonly string LuminanceValue = "return dot(value.rgb, float3(0.2125, 0.7154, 0.0721))";
        public static readonly string UniformWeightValue = "return dot(value.rgb, 1.0/3.0)";
        public static readonly string LightnessValue = @"
float lum = dot(value.rgb, float3(0.2125, 0.7154, 0.0721));
return max(116.0 * pow(max(lum, 0.0), 1.0 / 3.0) - 16.0, 0.0)";
        public static readonly string LumaValue = "return dot(toSrgb(value).rgb, float3(0.299, 0.587, 0.114))";
        public static readonly string AlphaValue = "return value.a";

        /// <summary>
        /// shader used for statistics calculation
        /// </summary>
        /// <param name="returnValue">one of the values declared above this function (*Value)</param>
        public StatisticsShader(UploadBuffer upload, string returnValue)
        {
            this.returnValue = returnValue;
            cbuffer = upload;

            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(ShaderBuilder.Builder2D), "StatisticsShader");
            shader3d = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(ShaderBuilder.Builder3D), "StatisticsShader");
        }

        /// <summary>
        /// puts statistic data of all pixels into the buffer
        /// </summary>
        internal void CopyToBuffer(ITexture source, GpuBuffer buffer, int layer = -1, int mipmap = 0)
        {
            // copy pixels from the source image into a texture from the texture cache
            var dev = Device.Get();
            if(source.Is3D) dev.Compute.Set(shader3d.Compute);
            else dev.Compute.Set(shader.Compute);

            var dim = source.Size.GetMip(mipmap);
            var numLayers = source.NumLayers;
            var curData = new StatisticsData
            {
                Level = mipmap,
                TrueBool = true
            };

            if (layer == -1)
            {
                dev.Compute.SetShaderResource(0, source.View);
            }
            else
            {
                // single layer
                dev.Compute.SetShaderResource(0, source.GetSrView(layer, mipmap));
                curData.Level = 0; // view with single level
                numLayers = 1;
            }
            cbuffer.SetData(curData);

            // buffer big enough?
            Debug.Assert(buffer.ElementCount >= dim.Product * numLayers);

            dev.Compute.SetUnorderedAccessView(0, buffer.View);
            dev.Compute.SetConstantBuffer(0, cbuffer.Handle);

            dev.Dispatch(Utility.Utility.DivideRoundUp(dim.Width, LocalSizeX), 
                Utility.Utility.DivideRoundUp(dim.Height, LocalSizeY),
                    Math.Max(dim.Depth, numLayers));

            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.SetShaderResource(0, null);
        }

        private string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvType} src_image : register(t0);
RWStructuredBuffer<float> dst_buffer : register(u0);

cbuffer InputBuffer : register(b0) {{
    uint level;
    bool trueBool;
}};

{Utility.Utility.ToSrgbFunction()}

struct ComputeIn {{
    uint3 global : SV_DispatchThreadID;
}};

bool isNan(float v) {{
    float vi = v;
    if(!trueBool) vi = 0.0; // true bool is always true, so this won't be executed

    // little trick to detect nans because v[i] != v[i] will be optimized away.
    return v != vi;
}}

float4 zeroNans(float4 v) {{
    float4 res = v;
    [unroll]
    for(uint i = 0; i < 4; ++i) {{
        if(isNan(v[i]))
            res[i] = 0.0;
    }}
    return res;
}}

float getPixel(int3 pos) {{
    float4 value = src_image.mips[level][pos];
    value = zeroNans(value);
    {returnValue};
}}

[numthreads({LocalSizeX}, {LocalSizeY}, 1)]
void main(ComputeIn i){{
    uint3 pos = i.global.xyz;
    uint width, height, depth, numLevels;
    src_image.GetDimensions(level, width, height, depth, numLevels);

    if(pos.x >= width || pos.y >= height) return;
    
    float value = getPixel(pos);
    
    dst_buffer[pos.z * width * height  + pos.y * width + pos.x] = value;
}}
";
        }

        public void Dispose()
        {
            shader?.Dispose();
            shader3d?.Dispose();
        }
    }
}
