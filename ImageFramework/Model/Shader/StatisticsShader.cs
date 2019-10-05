using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Utility;

namespace ImageFramework.Model.Shader
{
    public abstract class StatisticsShader<ResultT> : IDisposable
    {
        private DirectX.Shader shader;
        private readonly int LocalSize = 1; // TODO increase local size
        private readonly UploadBuffer<StatisticsData> cbuffer;

        protected StatisticsShader()
        {
            cbuffer = new UploadBuffer<StatisticsData>();
        }

        /// <param name="type">type of statistic for the debug name</param>
        protected void Init(string type)
        {
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute, GetSource(), type + "StatisticsShader");
        }

        internal ResultT Run(PixelValueShader pixelValueShader, TextureArray2D source, TextureCache cache, int layer = 0, int mipmap = 0)
        {
            var texDst = cache.GetTexture();
            
            // copy pixels from the source image into a texture from the texture cache
            var dev = Device.Get();
            dev.Compute.Set(shader.Compute);

            var curData = new StatisticsData
            {
                DirectionX = 1,
                DirectionY = 0,
                Width = source.GetWidth(mipmap),
                Height = source.GetHeight(mipmap),
                Stride = 2,
                Layer = layer,
                RefNan = float.NaN,
                FirstTime = true,
            };
            var curWidth = curData.Width;
            cbuffer.SetData(curData);

            dev.Compute.SetShaderResource(0, source.GetSrView(layer, mipmap));
            dev.Compute.SetUnorderedAccessView(0, texDst.GetUaView(mipmap));
            dev.Compute.SetConstantBuffer(0, cbuffer.Handle);

            dev.Dispatch(Utility.Utility.DivideRoundUp(curWidth, LocalSize * 2), curData.Height);

            // swap textures
            curData.FirstTime = false;
            var texSrc = cache.GetTexture();

            // do scan in x direction from right to left
            while (curWidth > 2)
            {
                curWidth = Utility.Utility.DivideRoundUp(curWidth, 2);
                curData.Stride *= 2;
                cbuffer.SetData(curData);

                // swap textures and rebind
                Swap(ref texSrc, ref texDst);
                UnbindResources();
                dev.Compute.SetUnorderedAccessView(0, texDst.GetUaView(mipmap));
                dev.Compute.SetShaderResource(0, texSrc.GetSrView(layer, mipmap));
                dev.Compute.SetConstantBuffer(0, cbuffer.Handle);

                dev.Dispatch(Utility.Utility.DivideRoundUp(curWidth, LocalSize * 2), curData.Height);
            }

            // do final scan in y direction
            curData.DirectionX = 0;
            curData.DirectionY = 1;
            curData.Stride = 2;

            var curHeight = curData.Height;
            while (curHeight > 1)
            {
                cbuffer.SetData(curData);

                // swap textures and rebind
                Swap(ref texSrc, ref texDst);
                UnbindResources();
                dev.Compute.SetUnorderedAccessView(0, texDst.GetUaView(mipmap));
                dev.Compute.SetShaderResource(0, texSrc.GetSrView(layer, mipmap));
                dev.Compute.SetConstantBuffer(0, cbuffer.Handle);

                dev.Dispatch(1, Utility.Utility.DivideRoundUp(curHeight, LocalSize * 2));

                curHeight = Utility.Utility.DivideRoundUp(curHeight, 2);
                curData.Stride *= 2;
            }

            UnbindResources();

            // the result is in pixel 0 0 
            var res = pixelValueShader.Run(texDst, 0, 0, layer, mipmap);

            // cleanup
            cache.StoreTexture(texSrc);
            cache.StoreTexture(texDst);

            return GetResult(res, curData.Width * curData.Height);
        }

        private void UnbindResources()
        {
            var dev = Device.Get();
            dev.Compute.SetUnorderedAccessView(0, null);
            dev.Compute.SetShaderResource(0, null);
        }

        private void Swap(ref TextureArray2D t1, ref TextureArray2D t2)
        {
            var tmp = t1;
            t1 = t2;
            t2 = tmp;
        }

        private string GetSource()
        {
            return $@"
Texture2D<float4> src_image : register(t0);
RWTexture2DArray<float4> dst_image : register(u0);

cbuffer InputBuffer : register(b0) {{
    uint2 direction;
    uint2 size;
    uint stride;
    uint layer;
    float refNan;
    bool firstTime;
}};

{GetFunctions()}

float4 combine(float4 a, float4 b) {{
    {GetCombineFunction()}
}}

float4 combineSingle(float4 a) {{
    {GetSingleCombine()}
}}

float4 oneTimeModify(float4 a) {{
    {GetOneTimeModifyFunction()}
}}

struct ComputeIn {{
    uint3 local : SV_GROUPTHREADID;
    uint3 group : SV_GROUPID;
}};

float4 zeroNans(float4 v) {{
    float4 res = v;
    [unroll]
    for(uint i = 0; i < 4; ++i) {{
        // little trick to detect nans because v[i] != v[i] will be optimized away.
        if(!(v[i] < 0.0) && !(v[i] > 0.0))
            res[i] = 0.0;
    }}
    return res;
}}

float4 getPixel(int2 pos) {{
    float4 value = src_image[pos];
    if(firstTime) {{
        // ignore nans
        value = zeroNans(value);
        value = oneTimeModify(value);
    }}
    return value;
}}

[numthreads({LocalSize}, 1, 1)]
void main(ComputeIn i){{
    const uint2 invDir = int2(1, 1) - direction;
    
    const uint2 pixelX = (dot(i.group.xy, direction) * {LocalSize} + i.local.x) * direction;
    const uint2 pixelY = dot(i.group.xy, invDir) * invDir;
    const uint2 pixel = pixelX + pixelY;

    const uint2 y = dot(pixel, invDir) * invDir;
    const uint2 x = dot(pixel, direction) * stride * direction;
    const uint2 x2 = x + direction * (stride / 2);

    const uint2 pos1 = x + y;
    const uint2 pos2 = x2 + y;

    if(pos1.x >= size.x || pos1.y >= size.y) return;
    
    float4 color;
    if(pos2.x >= size.x || pos2.y >= size.y) {{
        // only write the value as is
        color = combineSingle(getPixel(pos1));
    }} else {{
        color = combine(getPixel(pos1), getPixel(pos2));
    }}
    
    dst_image[uint3(pos1, layer)] = color;
}}
";
        }

        /// <summary>
        /// global functions
        /// </summary>
        protected virtual string GetFunctions()
        {
            return "";
        }

        /// <summary>
        /// this function will be called only once per pixel in the first iteration.
        /// default is: return a;
        /// </summary>
        protected virtual string GetOneTimeModifyFunction()
        {
            return "return a;";
        }

        /// <summary>
        /// this will be called to combine two pixel values into one.
        /// default: return a + b;
        /// </summary>
        protected virtual string GetCombineFunction()
        {
            return "return a + b;";
        }

        /// <summary>
        /// this will be called if there is no second pixel to use the combine function.
        /// default: return a;
        /// </summary>
        /// <returns></returns>
        protected virtual string GetSingleCombine()
        {
            return "return a;";
        }

        /// <summary>
        /// this function can modify the final result
        /// </summary>
        protected abstract ResultT GetResult(Color color, int numPixels);

        public void Dispose()
        {
            shader?.Dispose();
            cbuffer?.Dispose();
        }
    }
}
