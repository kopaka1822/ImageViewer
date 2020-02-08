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
                                                 GetFastSource(ShaderBuilder.Builder2D), "MipFast"));

        private DirectX.Shader FastShader3D => fastShader3D ?? (fastShader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetFastSource(ShaderBuilder.Builder3D), "MipFast3D"));

        private DirectX.Shader SlowShader => slowShader ?? (slowShader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetSlowSource(ShaderBuilder.Builder2D), "MipSlow"));

        private DirectX.Shader SlowShader3D => slowShader3D ?? (slowShader3D = new DirectX.Shader(DirectX.Shader.Type.Compute,
                                                 GetSlowSource(ShaderBuilder.Builder3D), "MipSlow3D"));

        private struct BufferData
        {
            public Size3 Dir; // direction of the filter
            public int NumSrcPixels; // number of pixels to process (per Kernel)
            public Size3 DstSize; // size of the destination image
            public int HasAlpha; // indicates if any pixel has alpha != 0
            public int NumSrcPixelsTotal; // src image dimension for the current direction
            public float FilterSize;// (float) number of pixels to process (per Kernel)
            public int Layer;
        }

        private readonly string weightFunc;
        private readonly int kernelStretch; // modifies the kernel size (1 = normal size)

        /// <param name="weightFunc">the weight func is actually the integral of the kernel function and will be evaluated between -1 and 1.
        /// This allows well defined handling of partially covered pixels. As opposed to sampling theory, a pixel is regarded as a square of constant value</param>
        /// <param name="kernelStretch">modifies the kernel size (1 = normal size)</param>
        protected DownscalingShaderBase(string weightFunc, int kernelStretch)
        {
            this.weightFunc = weightFunc;
            this.kernelStretch = kernelStretch;
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
                cbuffer.Layer = layer;
                cbuffer.DstSize = srcSize;
                cbuffer.DstSize.X = dstSize.X;

                ExecuteDimension(ref cbuffer, upload,  src.Is3D, 0, srcSize, src.GetSrView(layer, srcMipmap), tmpTex1.GetUaView(srcMipmap));
               // var tst = tmpTex1.GetPixelColors(layer, 0);

                cbuffer.DstSize.Y = dstSize.Y;
                if (src.Is3D)
                {
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 1, srcSize, tmpTex1.GetSrView(layer, srcMipmap), tmpTex2.GetUaView(srcMipmap));

                    cbuffer.DstSize.Z = dstSize.Z;
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 2, srcSize, tmpTex2.GetSrView(layer, srcMipmap), dst.GetUaView(dstMipmap));
                }
                else
                {
                    ExecuteDimension(ref cbuffer, upload, src.Is3D, 1, srcSize, tmpTex1.GetSrView(layer, srcMipmap), dst.GetUaView(dstMipmap));
                }
            }

            cache.StoreTexture(tmpTex1);
            if(tmpTex2 != null) cache.StoreTexture(tmpTex2);
        }

        private bool Odd(int num)
        {
            return num % 2 != 0;
        }

        private void ExecuteDimension(ref BufferData bufferData, UploadBuffer buffer, bool is3D, int dim, Size3 srcSize, ShaderResourceView srcTexture, UnorderedAccessView dstTexture)
        {
            // filter x direction
            bufferData.Dir = new Size3(0, 0, 0) {[dim] = 1};
            bufferData.NumSrcPixelsTotal = srcSize[dim];

            var dev = Device.Get();

            var iFilterSize = srcSize[dim] / bufferData.DstSize[dim];
            if (srcSize[dim] == bufferData.DstSize[dim]) // same size
            {
                dev.Compute.Set(is3D ? CopyShader3D.Compute : CopyShader.Compute);

                // just copy
            }
            else if (srcSize[dim] % bufferData.DstSize[dim] == 0 && // integer division possible? 
                     !(Odd(iFilterSize) && !Odd(kernelStretch))) // stretch does not result in half samples?
            {
                bufferData.NumSrcPixels = iFilterSize;
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
            BindAdditionalResources(dev);

            var builder = is3D ? ShaderBuilder.Builder3D : ShaderBuilder.Builder2D;

            dev.Dispatch(
                Utility.Utility.DivideRoundUp(bufferData.DstSize.X, builder.LocalSizeX),
                Utility.Utility.DivideRoundUp(bufferData.DstSize.Y, builder.LocalSizeY),
                Utility.Utility.DivideRoundUp(bufferData.DstSize.Z, builder.LocalSizeZ)
            );

            // unbind stuff
            dev.Compute.SetShaderResource(0, null);
            dev.Compute.SetUnorderedAccessView(0, null);


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
{HeaderAndMain(builder)}  {{
    {ReturnIfOutside()}
    
    dst_image[texel(id, layer)] = src_image[texel(id)];
}}
";
        }

        // source if the dst dimension is a multiple of the src dimension
        private string GetFastSource(IShaderBuilder builder)
        {
            return $@"
{AdditionalBindings}
{WeightFunc(weightFunc)}

{HeaderAndMain(builder)}  {{
    
    {ReturnIfOutside()}

    int fullFilterSize = numSrcPixels * {kernelStretch};
    int3 srcPos = id * dir * numSrcPixels + id * (1 - dir);
#if {kernelStretch} != 1
    int stretchOffset = uint(numSrcPixels * ({kernelStretch} - 1)) / 2;
    srcPos = srcPos - dir * stretchOffset;
#endif
    
    float factorScale = 2.0 / fullFilterSize;
    float factorOffset = fullFilterSize / 2.0;

    // sum up pixels from the src image
    {builder.Double}4 dstColor = 0.0;
    float leftWeight = weight(-1.0);
    for(int i = 1; i <= fullFilterSize; ++i){{
        // interpolation value [-1, 1] (i - size / 2) / (size / 2)
        float rightWeight = weight((float(i) - factorOffset) * factorScale);
        {AddWeightedPixel(builder)}
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

 {HeaderAndMain(builder)} {{
    
    {ReturnIfOutside()}
 
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
    {builder.Double}4 dstColor = 0.0;
    float leftWeight = weight(-1.0);
    for(int i = starti + 1; i <= endi; ++i){{
        // from [startf, endf] => [-1, 1]
        float rightWeight = weight(min((float(i) + factorOffset) * factorScale, 1.0));
        {AddWeightedPixel(builder)}
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
dst_image[texel(id, layer)] = float4(dstColor);
";
        }

        private string AddWeightedPixel(IShaderBuilder builder)
        {
            return $@"
float w = rightWeight - leftWeight;

#if {kernelStretch} != 1
float4 v = src_image[texel(clampCoord(srcPos))];
#else
float4 v = src_image[texel(srcPos)];
#endif
dstColor.a += {builder.Double}(v.a * w);
dstColor.rgb += {builder.Double}3(v.a * v.rgb * w);

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
    int3 dir; // direction of the filter
    int numSrcPixels; // number of pixels to process (per Kernel)
    uint3 dstSize; // size of the destination image
    bool hasAlpha; // indicates if any pixel has alpha != 0
    int numSrcPixelsTotal; // total number of src pixels for the current direction
    float filterSize; // (float) number of pixels to process (per Kernel)
    uint layer;
}};

int3 clampCoord(int3 coord) {{
    return clamp(dot(coord, dir), 0, numSrcPixelsTotal - 1) * dir + (1-dir) * coord;
}}

{builder.TexelHelperFunctions}

[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(uint3 id : SV_DispatchThreadID)";
        }

        private static string WeightFunc(string funcCore)
        {
            return $"float weight(float x) {{{funcCore}}}";
        }
    }
}
