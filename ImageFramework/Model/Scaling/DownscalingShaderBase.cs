using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Scaling
{
    public class DownscalingShaderBase : IDisposable
    {
        private DirectX.Shader copyShader;
        private DirectX.Shader copyShader3D;
        private DirectX.Shader fastShader;
        private DirectX.Shader fastShader3D;
        private DirectX.Shader slowShader;
        private DirectX.Shader slowShader3D;

        private DirectX.Shader CopyShader => copyShader ?? (copyShader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetCopySource(ShaderBuilder.Builder2D), "MipCopy"));

        private DirectX.Shader CopyShader3D => copyShader3D ?? (copyShader3D =
                                                   new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                       GetCopySource(ShaderBuilder.Builder3D), "MipCopy3D"));

        private DirectX.Shader FastShader => fastShader ?? (fastShader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetFastSource(ShaderBuilder.Builder2D, weightFunc), "MipFast"));

        private DirectX.Shader FastShader3D => fastShader3D ?? (fastShader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetFastSource(ShaderBuilder.Builder3D, weightFunc), "MipFast3D"));

        private DirectX.Shader SlowShader => slowShader ?? (slowShader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetSlowSource(ShaderBuilder.Builder2D, weightFunc), "MipSlow"));

        private DirectX.Shader SlowShader3D => slowShader3D ?? (slowShader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetSlowSource(ShaderBuilder.Builder3D, weightFunc), "MipSlow3D"));

        private struct BufferData
        {
            public Size3 Dir; // direction of the filter
            public int NumSrcPixels; // number of pixels to process (per Kernel)
            public Size3 DstSize; // size of the destination image
            public int HasAlpha; // indicates if any pixel has alpha != 0
            public double WeightSum;  // the sum of all weights (integral from -1 to 1)
            public int NumSrcPixelsTotal; // src image dimension for the current direction
            public float FilterSize;// (float) number of pixels to process (per Kernel)
            public int Layer;
        }

        private readonly float weightSum;
        private readonly string weightFunc;

        protected DownscalingShaderBase(float weightSum, string weightFunc)
        {
            this.weightSum = weightSum;
            this.weightFunc = weightFunc;
        }

        // for testing purposes
        internal void CompileShaders()
        {
            DirectX.Shader instance;
            instance = CopyShader;
            instance = CopyShader3D;
            instance = FastShader;
            instance = FastShader3D;
            instance = SlowShader;
            instance = SlowShader3D;
        }

        internal void Run(ITexture src, ITexture dst, int dstMipmap, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            Debug.Assert(cache.IsCompatibleWith(src));
            Debug.Assert(src.NumLayers == dst.NumLayers);
            Debug.Assert(dstMipmap < dst.NumMipmaps && dstMipmap >= 0);

            var dstSize = dst.Size.GetMip(dstMipmap);
            var cbuffer = new BufferData
            {
                HasAlpha = hasAlpha ? 1 : 0,
                WeightSum = weightSum
            };


            var tmpTex1 = cache.GetTexture();
            ITexture tmpTex2 = null;
            if (src.Is3D) tmpTex2 = cache.GetTexture();


            for (int layer = 0; layer < src.NumLayers; ++layer)
            {
                cbuffer.Layer = layer;
                cbuffer.DstSize = src.Size;
                cbuffer.DstSize.X = dstSize.X;

                ExecuteDimension(ref cbuffer, upload,  src.Is3D, 0, src.Size, src.GetSrView(layer, 0), tmpTex1.GetUaView(0));
               // var tst = tmpTex1.GetPixelColors(layer, 0);

                cbuffer.DstSize.Y = dstSize.Y;
                if (src.Is3D)
                {
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 1, src.Size, tmpTex1.GetSrView(layer, 0), tmpTex2.GetUaView(0));

                    cbuffer.DstSize.Z = dstSize.Z;
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 2, src.Size, tmpTex2.GetSrView(layer, 0), dst.GetUaView(dstMipmap));
                }
                else
                {
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 1, src.Size, tmpTex1.GetSrView(layer, 0), dst.GetUaView(dstMipmap));
                }
            }

            cache.StoreTexture(tmpTex1);
            if(tmpTex2 != null) cache.StoreTexture(tmpTex2);
        }

        private void ExecuteDimension(ref BufferData bufferData, UploadBuffer buffer, bool is3D, int dim, Size3 srcSize, ShaderResourceView srcTexture, UnorderedAccessView dstTexture)
        {
            // filter x direction
            bufferData.Dir = new Size3(0, 0, 0) {[dim] = 1};
            bufferData.NumSrcPixelsTotal = srcSize[dim];

            var dev = Device.Get();

            if (srcSize[dim] == bufferData.DstSize[dim]) // same size
            {
                dev.Compute.Set(is3D ? CopyShader3D.Compute : CopyShader.Compute);

                // just copy
            }
            else if (srcSize[dim] % bufferData.DstSize[dim] == 0) // integer division possible? 
            {
                bufferData.NumSrcPixels = srcSize[dim] / bufferData.DstSize[dim];
                dev.Compute.Set(is3D ? FastShader3D.Compute : FastShader.Compute);
            }
            else
            {
                bufferData.FilterSize = srcSize[dim] / (float)bufferData.DstSize[dim];
                dev.Compute.Set(is3D ? SlowShader3D.Compute : SlowShader.Compute);
            }

            // bind stuff
            buffer.SetData(bufferData);
            dev.Compute.SetConstantBuffer(0, buffer.Handle);
            dev.Compute.SetShaderResource(0, srcTexture);
            dev.Compute.SetUnorderedAccessView(0, dstTexture);

            var builder = is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;

            dev.Dispatch(
                Utility.Utility.DivideRoundUp(bufferData.DstSize.X, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(bufferData.DstSize.Y, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(bufferData.DstSize.Z, builder.LocalSizeZ)
            );

            // unbind stuff
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);


        }

        public void Dispose()
        {
            copyShader?.Dispose();
            copyShader3D?.Dispose();
            fastShader?.Dispose();
            fastShader3D?.Dispose();
            slowShader?.Dispose();
            slowShader3D?.Dispose();
        }

        // just copies pixels because the dimension match (numSrcPixel == dir*dstSize)
        public static string GetCopySource(IShaderBuilder builder)
        {
            return $@"
{HeaderAndMain(builder)}  {{
    {ReturnIfOutside()}
    
    dst_image[texel(id, layer)] = src_image[texel(id)];
}}
";
        }

        // source if the dst dimension is a multiple of the src dimension
        public static string GetFastSource(IShaderBuilder builder, string weightFunc)
        {
            return $@"
{WeightFunc(weightFunc)}

{HeaderAndMain(builder)}  {{
    
    {ReturnIfOutside()}

    uint3 srcPos = id * dir * numSrcPixels + id * (1 - dir);
    float factorScale = 2.0 / numSrcPixels;
    float factorOffset = numSrcPixels / 2.0;

    // sum up pixels from the src image
    double4 dstColor = 0.0;
    float leftWeight = weight(-1.0);
    for(uint i = 1; i <= numSrcPixels; ++i){{
        // interpolation value [-1, 1] (i - size / 2) / (size / 2)
        float rightWeight = weight((float(i) - factorOffset) * factorScale);
        {AddWeightedPixel()}
    }}    
    
    {NormalizeAndWriteBackColor()}
}}
";
        }

        // source if the dst dimension is not a multiple of the src dimension (special border handling)
        public static string GetSlowSource(IShaderBuilder builder, string weightFunc)
        {
            return $@"
{WeightFunc(weightFunc)}

 {HeaderAndMain(builder)} {{
    
    {ReturnIfOutside()}
 
    // interval in float coordinates
    float startf = dot(id, dir) * filterSize;
    float endf = (dot(id, dir) + 1) * filterSize;
    
    uint starti = floor(startf);
    uint endi = max(ceil(endf), numSrcPixelsTotal);
    uint3 srcPos = dir * starti + (1 - dir) * id;

    float factorScale = 2.0 / filterSize;
    float factorOffset = -startf - filterSize / 2.0;

    // sum up pixels
    double4 dstColor = 0.0;
    float leftWeight = weight(-1.0);
    for(uint i = starti + 1; i <= endi; ++i){{
        // from [startf, endf] => [-1, 1]
        float rightWeight = weight(min((float(i) + factorOffset) * factorScale, 1.0));
        {AddWeightedPixel()}
    }}
    
    {NormalizeAndWriteBackColor()}
}}
";
        }

        private static string NormalizeAndWriteBackColor()
        {
            return @"
dstColor /= weightSum;
if(!hasAlpha) dstColor.a = 1.0; // not always true due to precision errors
if(dstColor.a != 0.0) dstColor.rgb /= dstColor.a;

// write back color
dst_image[texel(id, layer)] = float4(dstColor);
";
        }

        private static string AddWeightedPixel()
        {
            return @"
float w = rightWeight - leftWeight;

float4 v = src_image[texel(srcPos)];
dstColor.a += (double)(v.a * w);
dstColor.rgb += (double)(v.a * v.rgb * w);

srcPos += dir;
leftWeight = rightWeight;
";
        }

        private static string ReturnIfOutside()
        {
            return "if(any(id >= dstSize)) return;";
        }

        private static string HeaderAndMain(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);
{builder.UavType} dst_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    uint3 dir; // direction of the filter
    uint numSrcPixels; // number of pixels to process (per Kernel)
    uint3 dstSize; // size of the destination image
    bool hasAlpha; // indicates if any pixel has alpha != 0
    double weightSum; // the sum of all weights (integral from -1 to 1)
    uint numSrcPixelsTotal; // total number of src pixels for the current direction
    float filterSize; // (float) number of pixels to process (per Kernel)
    uint layer;
}};

// texel helper function
#if {builder.Is3DInt}
uint3 texel(uint3 coord) {{ return coord; }}
uint3 texel(uint3 coord, uint layer) {{ return coord; }}
#else
uint2 texel(uint3 coord) {{ return coord.xy; }}
uint3 texel(uint3 coord, uint layer) {{ return uint3(coord.xy, layer); }}
#endif

[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(uint3 id : SV_DispatchThreadID)";
        }

        private static string WeightFunc(string funcCore)
        {
            return $"float weight(float x) {{{funcCore}}}";
        }
    }
}
