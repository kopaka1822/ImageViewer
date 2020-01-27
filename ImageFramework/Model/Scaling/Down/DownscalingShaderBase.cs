using System;
using System.Diagnostics;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Scaling.Down
{
    internal class DownscalingShaderBase : IDownscalingShader
    {
        private readonly QuadShader quad;
        private DirectX.Shader copyShader;
        private DirectX.Shader copyShader3D;
        private DirectX.Shader fastShader;
        private DirectX.Shader fastShader3D;
        private DirectX.Shader slowShader;
        private DirectX.Shader slowShader3D;

        private DirectX.Shader CopyShader => copyShader ?? (copyShader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetCopySource(ShaderBuilder.Builder2D), "MipCopy"));

        private DirectX.Shader CopyShader3D => copyShader3D ?? (copyShader3D =
                                                   new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                       GetCopySource(ShaderBuilder.Builder3D), "MipCopy3D"));

        private DirectX.Shader FastShader => fastShader ?? (fastShader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetFastSource(ShaderBuilder.Builder2D), "MipFast"));

        private DirectX.Shader FastShader3D => fastShader3D ?? (fastShader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetFastSource(ShaderBuilder.Builder3D), "MipFast3D"));

        private DirectX.Shader SlowShader => slowShader ?? (slowShader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetSlowSource(ShaderBuilder.Builder2D), "MipSlow"));

        private DirectX.Shader SlowShader3D => slowShader3D ?? (slowShader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                                 GetSlowSource(ShaderBuilder.Builder3D), "MipSlow3D"));

        private struct BufferData
        {
            public Size3 Dir; // direction of the filter
            public int NumSrcPixels; // number of pixels to process (per Kernel)
            public Size3 DstSize; // size of the destination image
            public int HasAlpha; // indicates if any pixel has alpha != 0
            public int NumSrcPixelsTotal; // src image dimension for the current direction
            public float FilterSize;// (float) number of pixels to process (per Kernel)
        }

        private readonly string weightFunc;
        private readonly int kernelStretch; // modifies the kernel size (1 = normal size)

        /// <param name="weightFunc">the weight func is actually the integral of the kernel function and will be evaluated between -1 and 1.
        /// This allows well defined handling of partially covered pixels. As opposed to sampling theory, a pixel is regarded as a square of constant value</param>
        /// <param name="kernelStretch">modifies the kernel size (1 = normal size)</param>
        protected DownscalingShaderBase(string weightFunc, int kernelStretch, QuadShader quad)
        {
            this.weightFunc = weightFunc;
            this.kernelStretch = kernelStretch;
            this.quad = quad;
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

        public void Run(ITexture src, ITexture dst, int srcMipmap, int dstMipmap, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            Debug.Assert(cache.IsCompatibleWith(src));
            Debug.Assert(src.NumLayers == dst.NumLayers);
            Debug.Assert(dstMipmap < dst.NumMipmaps && dstMipmap >= 0);
            Debug.Assert(srcMipmap < src.NumMipmaps && dstMipmap >= 0);

            var dstSize = dst.Size.GetMip(dstMipmap);
            var srcSize = src.Size.GetMip(srcMipmap);
            var cbuffer = new BufferData
            {
                HasAlpha = hasAlpha ? 1 : 0
            };


            var tmpTex1 = cache.GetTexture();
            ITexture tmpTex2 = null;
            if (src.Is3D) tmpTex2 = cache.GetTexture();


            for (int layer = 0; layer < src.NumLayers; ++layer)
            {
                cbuffer.DstSize = srcSize;
                cbuffer.DstSize.X = dstSize.X;

                ExecuteDimension(ref cbuffer, upload,  src.Is3D, 0, srcSize, src.GetSrView(layer, srcMipmap), tmpTex1.GetRtView(layer, srcMipmap));
               // var tst = tmpTex1.GetPixelColors(layer, 0);

                cbuffer.DstSize.Y = dstSize.Y;
                if (src.Is3D)
                {
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 1, srcSize, tmpTex1.GetSrView(layer, srcMipmap), tmpTex2.GetRtView(layer, srcMipmap));

                    cbuffer.DstSize.Z = dstSize.Z;
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 2, srcSize, tmpTex2.GetSrView(layer, srcMipmap), dst.GetRtView(layer, dstMipmap));
                }
                else
                {
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 1, srcSize, tmpTex1.GetSrView(layer, srcMipmap), dst.GetRtView(layer, dstMipmap));
                }
            }

            cache.StoreTexture(tmpTex1);
            if(tmpTex2 != null) cache.StoreTexture(tmpTex2);
        }

        private bool Odd(int num)
        {
            return num % 2 != 0;
        }

        private void ExecuteDimension(ref BufferData bufferData, UploadBuffer buffer, bool is3D, int dim, Size3 srcSize, ShaderResourceView srcTexture, RenderTargetView dstTexture)
        {
            // filter x direction
            bufferData.Dir = new Size3(0, 0, 0) {[dim] = 1};
            bufferData.NumSrcPixelsTotal = srcSize[dim];

            var dev = Device.Get();
            quad.Bind(is3D);

            var iFilterSize = srcSize[dim] / bufferData.DstSize[dim];
            if (srcSize[dim] == bufferData.DstSize[dim]) // same size
            {
                dev.Pixel.Set(is3D ? CopyShader3D.Pixel : CopyShader.Pixel);

                // just copy
            }
            else if (srcSize[dim] % bufferData.DstSize[dim] == 0 && // integer division possible? 
                     !(Odd(iFilterSize) && !Odd(kernelStretch))) // stretch does not result in half samples?
            {
                bufferData.NumSrcPixels = iFilterSize;
                dev.Pixel.Set(is3D ? FastShader3D.Pixel : FastShader.Pixel);
            }
            else
            {
                bufferData.FilterSize = srcSize[dim] / (float)bufferData.DstSize[dim];
                dev.Pixel.Set(is3D ? SlowShader3D.Pixel : SlowShader.Pixel);
            }

            // bind stuff
            buffer.SetData(bufferData);
            dev.Pixel.SetConstantBuffer(0, buffer.Handle);
            dev.Pixel.SetShaderResource(0, srcTexture);
            BindAdditionalResources(dev);

            dev.OutputMerger.SetRenderTargets(dstTexture);
            dev.SetViewScissors(bufferData.DstSize.X, bufferData.DstSize.Y);
            dev.DrawFullscreenTriangle(bufferData.DstSize.Z);

            // unbind stuff
            quad.Unbind();
            dev.Pixel.Set(null);
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            UnbindAdditionalResources(dev);
        }

        public virtual void Dispose()
        {
            copyShader?.Dispose();
            copyShader3D?.Dispose();
            fastShader?.Dispose();
            fastShader3D?.Dispose();
            slowShader?.Dispose();
            slowShader3D?.Dispose();
        }

        protected string AdditionalBindings { get; set; } = "";

        protected virtual void BindAdditionalResources(Device dev) {}

        protected virtual void UnbindAdditionalResources(Device dev) {}

        // just copies pixels because the dimension match (numSrcPixel == dir*dstSize)
        public static string GetCopySource(IShaderBuilder builder)
        {
            return $@"
{HeaderAndMain(builder)}
    return src_image[texel(id)];
}}
";
        }

        // source if the dst dimension is a multiple of the src dimension
        private string GetFastSource(IShaderBuilder builder)
        {
            return $@"
{AdditionalBindings}
{WeightFunc(weightFunc)}

{HeaderAndMain(builder)}
    int fullFilterSize = numSrcPixels * {kernelStretch};
    int3 srcPos = id * dir * numSrcPixels + id * (1 - dir);
#if {kernelStretch} != 1
    int stretchOffset = uint(numSrcPixels * ({kernelStretch} - 1)) / 2;
    srcPos = srcPos - dir * stretchOffset;
#endif
    
    float factorScale = 2.0 / fullFilterSize;
    float factorOffset = fullFilterSize / 2.0;

    // sum up pixels from the src image
    double4 dstColor = 0.0;
    float leftWeight = weight(-1.0);
    for(int i = 1; i <= fullFilterSize; ++i){{
        // interpolation value [-1, 1] (i - size / 2) / (size / 2)
        float rightWeight = weight((float(i) - factorOffset) * factorScale);
        {AddWeightedPixel()}
    }}    
    
    {NormalizeAndWriteBackColor()}
}}
";
        }

        // source if the dst dimension is not a multiple of the src dimension (special border handling)
        public string GetSlowSource(IShaderBuilder builder)
        {
            return $@"
{AdditionalBindings}
{WeightFunc(weightFunc)}

 {HeaderAndMain(builder)}
    // interval in float coordinates
    float startf = dot(id, dir) * filterSize;
    float endf = (dot(id, dir) + 1) * filterSize;
    
#if {kernelStretch} != 1
    float stretchOffset = (filterSize * ({kernelStretch} - 1)) / 2.0;
    startf -= stretchOffset;
    endf += stretchOffset;
#endif

    uint starti = floor(startf);

#if {kernelStretch} == 1
    int endi = min(ceil(endf), numSrcPixelsTotal);
#else
    int endi = ceil(endf);
#endif
    int3 srcPos = dir * starti + (1 - dir) * id;

    float factorScale = 2.0 / (filterSize * {kernelStretch});
    float factorOffset = -startf - (filterSize * {kernelStretch}) / 2.0;

    // sum up pixels
    double4 dstColor = 0.0;
    float leftWeight = weight(-1.0);
    for(int i = starti + 1; i <= endi; ++i){{
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
dstColor /= (weight(1.0)-weight(-1.0));
if(!hasAlpha) dstColor.a = 1.0; // not always true due to precision errors
if(dstColor.a != 0.0) dstColor.rgb /= dstColor.a;

// write back color
return float4(dstColor);
";
        }

        private string AddWeightedPixel()
        {
            return $@"
float w = rightWeight - leftWeight;

#if {kernelStretch} != 1
float4 v = src_image[texel(clampCoord(srcPos))];
#else
float4 v = src_image[texel(srcPos)];
#endif
dstColor.a += double(v.a * w);
dstColor.rgb += double3(v.a * v.rgb * w);

srcPos += dir;
leftWeight = rightWeight;
";
        }

        private static string HeaderAndMain(IShaderBuilder builder)
        {
            return $@"
{builder.SrvSingleType} src_image : register(t0);

cbuffer InputBuffer : register(b0) {{
    int3 dir; // direction of the filter
    int numSrcPixels; // number of pixels to process (per Kernel)
    uint3 dstSize; // size of the destination image
    bool hasAlpha; // indicates if any pixel has alpha != 0
    int numSrcPixelsTotal; // total number of src pixels for the current direction
    float filterSize; // (float) number of pixels to process (per Kernel)
}};

int3 clampCoord(int3 coord) {{
    return clamp(dot(coord, dir), 0, numSrcPixelsTotal - 1) * dir + (1-dir) * coord;
}}

{builder.TexelHelperFunctions}

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
";
        }

        private static string WeightFunc(string funcCore)
        {
            return $"float weight(float x) {{{funcCore}}}";
        }
    }
}
