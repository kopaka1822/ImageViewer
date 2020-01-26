using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Scaling.Down
{
    // core of the detail preserving shader
    internal class DetailPreservingShaderCore : IDisposable
    {
        private readonly QuadShader quad;
        private DirectX.Shader fastShader;
        private DirectX.Shader fastShader3D;
        private DirectX.Shader slowShader;
        private DirectX.Shader slowShader3D;
        private readonly bool isDetailed;

        private DirectX.Shader FastShader => fastShader ?? (fastShader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetFastSource(ShaderBuilder.Builder2D, isDetailed), "DetailFast"));
        private DirectX.Shader FastShader3D => fastShader3D ?? (fastShader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetFastSource(ShaderBuilder.Builder3D, isDetailed), "DetailFast3D"));
        private DirectX.Shader SlowShader => slowShader ?? (slowShader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                   GetSlowSource(ShaderBuilder.Builder2D, isDetailed), "DetailSlow"));
        private DirectX.Shader SlowShader3D => slowShader3D ?? (slowShader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                   GetSlowSource(ShaderBuilder.Builder3D, isDetailed), "DetailSlow3D"));

        public DetailPreservingShaderCore(bool isDetailed, QuadShader quad)
        {
            this.isDetailed = isDetailed;
            this.quad = quad;
        }

        private struct BufferData
        {
            public Size3 SrcSize;
            public int HasAlpha;
            public Size3 DstSize;
#pragma warning disable 169
            private int Padding0;
#pragma warning restore 169
            public float FilterSizeFloatX;
            public float FilterSizeFloatY;
            public float FilterSizeFloatZ;
        }

        public void Run(ITexture src, ITexture guide, ITexture dst, int srcMipmap, int dstMipmap, bool hasAlpha, UploadBuffer upload)
        {
            Debug.Assert(guide.Size == dst.Size);
            Debug.Assert(guide.NumMipmaps >= dstMipmap);
            Debug.Assert(dst.NumMipmaps >= dstMipmap);
            Debug.Assert(guide.NumLayers == dst.NumLayers);
            Debug.Assert(src.NumLayers == dst.NumLayers);

            var bufferData = new BufferData
            {
               SrcSize = src.Size.GetMip(srcMipmap),
               DstSize = dst.Size.GetMip(dstMipmap),
               HasAlpha = hasAlpha?1:0,
            };
            bufferData.FilterSizeFloatX = bufferData.SrcSize.X / (float)bufferData.DstSize.X;
            bufferData.FilterSizeFloatY = bufferData.SrcSize.Y / (float)bufferData.DstSize.Y;
            bufferData.FilterSizeFloatZ = bufferData.SrcSize.Z / (float)bufferData.DstSize.Z;

            // select shader and builder
            var builder = src.Is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;
            bool isFastShader = (bufferData.SrcSize.X % bufferData.DstSize.X == 0)
                                && (bufferData.SrcSize.Y % bufferData.DstSize.Y == 0)
                                && (bufferData.SrcSize.Z % bufferData.DstSize.Z == 0);

            quad.Bind(src.Is3D);
            DirectX.Shader shader;
            if (isFastShader) shader = src.Is3D ? FastShader3D : FastShader;          
            else shader = src.Is3D ? SlowShader3D : SlowShader;

            var dev = Device.Get();
            dev.Pixel.Set(shader.Pixel);

            for (int layer = 0; layer < src.NumLayers; ++layer)
            {
                upload.SetData(bufferData);
                dev.Pixel.SetConstantBuffer(0, upload.Handle);
                dev.Pixel.SetShaderResource(0, src.GetSrView(layer, srcMipmap));
                dev.Pixel.SetShaderResource(1, guide.GetSrView(layer, dstMipmap));

                dev.OutputMerger.SetRenderTargets(dst.GetRtView(layer, dstMipmap));
                dev.SetViewScissors(bufferData.DstSize.X, bufferData.DstSize.Y);
                dev.DrawFullscreenTriangle(bufferData.DstSize.Z);
            }

            quad.Unbind();
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
        }

        internal void CompileShaders()
        {
            DirectX.Shader s;
            s = FastShader;
            s = FastShader3D;
            s = SlowShader;
            s = slowShader3D;
        }

        private static string HeaderAndMain(IShaderBuilder builder, bool veryDetailed)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);
{builder.SrvSingleType} guide : register(t1);

cbuffer InputBuffer : register(b0) {{
    uint3 srcSize;
    bool hasAlpha;
    uint3 dstSize;
    int padding0;
    float3 filterSizeFloat;
}};

{Utility.Utility.ToSrgbFunction()}

static float4 guideValue; // in sRGB space (perceived difference)

// color: color in sRGB space
float weight(float4 color) {{
    // according to paper:
    // (length(guide - color) / Vmax) ^ y with y == 0.5, or y == 1.0 for very detailed
    // Vmax = maximal color difference

    // sRGB difference
    float3 diff = (color.rgb - guideValue.rgb); // TODO multiply with alpha?
    diff *= diff;
    // give luma weights to each color => assuming max difference of (1, 1, 1) leads to weightedDiff [0, 1] => Vmax = 1
    float weightedDiff = dot(diff, float3(0.299, 0.587, 0.114));
    // weightedDiff ~ length(diff) ^ 2
    return {(veryDetailed ? "sqrt(weightedDiff)" : "pow(weightedDiff, 0.25)")};
}}

// indicates how much of the box is covered
float getVisibility(float position, float start, float end)
{{
	if(position < start) return position + 1.0 - start;
	return min(end - position, 1.0);
}}

{builder.TexelHelperFunctions}

float weight();

struct PixelIn
{{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
#if {builder.Is3DInt}
    uint depth : SV_RenderTargetArrayIndex;
#endif
}};

float4 main(PixelIn pin) : SV_TARGET {{
    int3 id = int3(pin.projPos.xy, 0);
#if {builder.Is3DInt}
    id.z = pin.depth;
#endif

    guideValue = toSrgb(guide[texel(id)]);

    double4 dcolor = 0.0;
    double weightSum = 0.0;
";
        }

        private static string GetFastSource(IShaderBuilder builder, bool veryDetailed)
        {
            return $@"
{HeaderAndMain(builder, veryDetailed)}
    
    uint3 startPos = id * (srcSize / dstSize);
    uint3 endPos = (id + 1) * (srcSize / dstSize);
    uint3 coord = 0;
    for(coord.z = startPos.z; coord.z < endPos.z; ++coord.z)
    for(coord.y = startPos.y; coord.y < endPos.y; ++coord.y)
    for(coord.x = startPos.x; coord.x < endPos.x; ++coord.x) {{
        float4 c = src_image[texel(coord)];
        float w = weight(toSrgb(c));
        weightSum += w;
        dcolor.a += double(c.a * w);
        dcolor.rgb += double3(c.a * c.rgb * w);
    }}

    {NormalizeAndWriteBackColor()}
}}";          
        }

        private static string GetSlowSource(IShaderBuilder builder, bool veryDetailed)
        {
            return $@"
{HeaderAndMain(builder, veryDetailed)}

    float3 startPosf = id * filterSizeFloat;
    float3 endPosf = (id + 1) * filterSizeFloat;
    uint3 startPos = floor(startPosf);
    uint3 endPos = min(ceil(endPosf), srcSize);    

    uint3 coord = 0;
    for(coord.z = startPos.z; coord.z < endPos.z; ++coord.z)
    for(coord.y = startPos.y; coord.y < endPos.y; ++coord.y) {{
        // coverage of the current pixel
        const float yzVis = getVisibility(coord.z, startPosf.z, endPosf.z) *  getVisibility(coord.y, startPosf.y, endPosf.y);
        for(coord.x = startPos.x; coord.x < endPos.x; ++coord.x) {{
            float4 c = src_image[texel(coord)];
            // multiply weight by pixel coverage
            float w = weight(toSrgb(c)) * yzVis * getVisibility(coord.x, startPosf.x, endPosf.x);
            weightSum += w;
            dcolor.a += double(c.a * w);
            dcolor.rgb += double3(c.a * c.rgb * w);
        }}
    }}

    {NormalizeAndWriteBackColor()}
}}";
        }

        private static string NormalizeAndWriteBackColor()
        {
            return @" 
dcolor /= weightSum;
if(!hasAlpha) dcolor.a = 1.0;
if(dcolor.a != 0.0) dcolor.rgb /= dcolor.a;

if(weightSum <= 0.0) // there was not difference in color => take guide value (all pixels were equal to the guide)
    dcolor = guide[texel(id)];

return float4(dcolor);";
        }

        public void Dispose()
        {
            fastShader?.Dispose();
            fastShader3D?.Dispose();
            slowShader?.Dispose();
            slowShader3D?.Dispose();
        }
    }
}
