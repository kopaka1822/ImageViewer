using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Shader
{
    /// <summary>
    /// shader that copies a single float for each texel into a buffer
    /// </summary>
    public class StatisticsShader : IDisposable
    {
        private readonly DirectX.Shader shader;
        private readonly DirectX.Shader shader3d;
        private readonly int LocalSizeX = 128;
        private readonly int LocalSizeY = 4;
        private readonly UploadBuffer cbuffer;
        private readonly string returnValue;
        private readonly bool multisample = false; // if true, the shader will evaluate 4x4 positions inside a pixel with linear interpolation

        // (optional) user parameter than can be used for the return value
        public float UserParameter { get; set; } = 0.0f;

        private SamplerState linearSampler = null;

        // predefined return values
        public static readonly string LuminanceValue = "return dot(value.a * value.rgb, float3(0.2125, 0.7154, 0.0721))";
        public static readonly string UniformWeightValue = "return dot(value.a * value.rgb, 1.0/3.0)";
        public static readonly string LightnessValue = @"
float lum = dot(value.a * value.rgb, float3(0.2125, 0.7154, 0.0721));
return max(116.0 * pow(max(lum, 0.0), 1.0 / 3.0) - 16.0, 0.0)";
        public static readonly string LumaValue = "return dot(value.a * toSrgb(value).rgb, float3(0.299, 0.587, 0.114))";
        public static readonly string AlphaValue = "return value.a";
        public static readonly string RedValue = "return value.r";
        public static readonly string AlphaTestCoverage = "return ((value.a < userParameter) ? 0.0 : 1.0)";
        public static readonly string GrayscaleValue = "return (value.r == value.g && value.r == value.b) ? 1.0 : 0.0";

        /// <summary>
        /// shader used for statistics calculation
        /// </summary>
        /// <param name="returnValue">one of the values declared above this function (*Value)</param>
        /// <param name="multisample">uses multisampling to evaluate the expression. For 2D it will bi-linear interpolate 4x4 position</param>
        public StatisticsShader(UploadBuffer upload, string returnValue, bool multisample = false)
        {
            this.returnValue = returnValue;
            this.multisample = multisample;
            cbuffer = upload;

            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(ShaderBuilder.Builder2D), "StatisticsShader");
            shader3d = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(ShaderBuilder.Builder3D), "StatisticsShader");
        }

        internal void CopyToBuffer(ITexture source, GpuBuffer buffer, LayerMipmapRange lm)
        => CopyToBuffer(source, buffer, lm, Size3.Zero);

        /// <summary>
        /// puts statistic data of all pixels into the buffer
        /// </summary>
        /// <param name="lm">range with single mipmap</param>
        /// <param name="offset">offset in each direction</param>
        /// <param name="source"></param>
        /// <param name="buffer"></param>
        internal void CopyToBuffer(ITexture source, GpuBuffer buffer, LayerMipmapRange lm, Size3 offset)
        {
            Debug.Assert(lm.IsSingleMipmap);

            // copy pixels from the source image into a texture from the texture cache
            var dev = Device.Get();
            if(source.Is3D) dev.Compute.Set(shader3d.Compute);
            else dev.Compute.Set(shader.Compute);

            var dim = source.Size.GetMip(lm.Mipmap);
            AdjustDim(ref dim, ref offset, source.Is3D);

            var numLayers = source.LayerMipmap.Layers;
            var curData = new BufferData
            {
                Level = lm.Mipmap,
                TrueBool = true,
                Offset = offset,
                Size = dim,
                Resolution = source.Size.GetMip(lm.Mipmap),
                UserParameter = UserParameter,
            };

            if (lm.AllLayer)
            {
                dev.Compute.SetShaderResource(0, source.View);
            }
            else
            {
                // single layer
                dev.Compute.SetShaderResource(0, source.GetSrView(lm.Single));
                curData.Level = 0; // view with single level
                numLayers = 1;
            }
            cbuffer.SetData(curData);

            // buffer big enough?
            Debug.Assert(buffer.ElementCount >= dim.Product * numLayers);

            dev.Compute.SetUnorderedAccessView(0, buffer.View);
            dev.Compute.SetConstantBuffer(0, cbuffer.Handle);

            if (multisample) BindLinearSampler(dev); // only required for multisampling

            dev.Dispatch(Utility.Utility.DivideRoundUp(dim.Width, LocalSizeX), 
                Utility.Utility.DivideRoundUp(dim.Height, LocalSizeY),
                    Math.Max(dim.Depth, numLayers));

            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetSampler(0, null);
        }

        private void BindLinearSampler(Device dev)
        {
            if (linearSampler == null)
            {
                // assume wrap for our purposes
                linearSampler = new SamplerState(dev.Handle, new SamplerStateDescription
                {
                    AddressU = TextureAddressMode.Wrap,
                    AddressV = TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,
                    Filter = SharpDX.Direct3D11.Filter.MinMagLinearMipPoint, // linear but nearest mipmap
                    MaximumLod = float.MaxValue,
                    MinimumLod = 0,
                    MipLodBias = 0,
                    ComparisonFunction = Comparison.Never,
                    MaximumAnisotropy = 1,
                    BorderColor = new RawColor4(0, 0, 0, 0),
                });
            }
            dev.Compute.SetSampler(0, linearSampler);
        }

        private void AdjustDim(ref Size3 dim, ref Size3 offset, bool is3D)
        {
            var fullDim = dim;

            dim.X = Math.Max(dim.X - 2 * offset.X, 1);
            if (dim.X == 1) offset.X = fullDim.X / 2;

            dim.Y = Math.Max(dim.Y - 2 * offset.Y, 1);
            if (dim.Y == 1) offset.Y = fullDim.Y / 2;

            if (is3D) dim.Z = Math.Max(dim.Z - 2 * offset.Z, 1);
            if (dim.Z == 1) offset.Z = fullDim.Z / 2;
        }

        /// <summary>
        /// calculates the number of buffer elements required with the current configuation
        /// </summary>
        public int GetRequiredElementCount(ITexture tex, LayerMipmapRange lm, Size3 offset)
        {
            var dim = tex.Size.GetMip(lm.SingleMipmap);
            AdjustDim(ref dim, ref offset, tex.Is3D);
            var res = dim.Product;
            if (lm.AllLayer) res *= tex.NumLayers;
            return res;
        }

        private struct BufferData
        {
            public Size3 Offset;
            public int Level;
            public Size3 Size; // size of the region to be processed ()
            public RawBool TrueBool;
            public Size3 Resolution; // Resolution of the texture
            public float UserParameter; // optional, can be supplied for the return value
        }

        private string GetSource(IShaderBuilder builder)
        {
            return $@"
{(multisample ? "#define MULTISAMPLE":"")}

{builder.SrvType} src_image : register(t0);
RWStructuredBuffer<float> dst_buffer : register(u0);

SamplerState sampler_linear : register(s0);

cbuffer InputBuffer : register(b0) {{
    uint3 offset;
    uint level;
    uint3 size;
    bool trueBool;
    uint3 resolution;
    float userParameter;
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

float processPixel(float4 value)
{{
    value = zeroNans(value);
    {returnValue};
}}

[numthreads({LocalSizeX}, {LocalSizeY}, 1)]
void main(ComputeIn i){{
    uint3 pos = i.global.xyz;

    if(pos.x >= size.x || pos.y >= size.y) return;
    
    #ifdef MULTISAMPLE
    float value = 0.0;
    uint N = {(builder.Is3D ? "3" : "4")}; // 4x4 samples for 2d, 3x3x3 for 3d
    uint3 s = 0u;
    {(builder.Is3D?"for(s.z = 0; s.z < N; ++s.z)":"")}
    for(s.y = 0; s.y < N; ++s.y)
    for(s.x = 0; s.x < N; ++s.x)
    {{
        float3 pixelF = float3(pos + offset) + 1.0 / (2.0 * float(N)) + float3(s) / float(N);
        pixelF = pixelF / float3(resolution);
        value += processPixel(src_image.SampleLevel(sampler_linear, pixelF, level));
    }}
    {(builder.Is3D ? "value /= float(N*N*N);" : "value /= float(N*N);")} // normalize value

    #else // no multisample => single pixel fetch
    float value = processPixel(src_image.mips[level][pos + offset]);
    #endif    

    dst_buffer[pos.z * size.x * size.y  + pos.y * size.x + pos.x] = value;
}}
";
        }

        public void Dispose()
        {
            linearSampler?.Dispose();
            shader?.Dispose();
            shader3d?.Dispose();
        }
    }
}
