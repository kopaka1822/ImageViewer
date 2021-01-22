using System;
using System.Security.Cryptography;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model.Equation.Token;

namespace ImageFramework.Model.Shader
{
    internal class ImageCombineShader : IDisposable
    {
        private DirectX.Shader shader;
        private readonly IShaderBuilder builder;

        public ImageCombineShader(string colorFormula, string alphaFormula, int numImages, IShaderBuilder builder)
        {
            this.builder = builder;
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                GetShaderSource(colorFormula, alphaFormula, Math.Max(numImages, 1), builder), "ImageCombineShader");
        }

        private struct InfoBuffer
        {
            public int Layer;
            public int Level;
            public float NaNValue;
        }

        public void Run(ImagesModel images, UploadBuffer constantBuffer, ITexture target, int numMipmaps)
        {
            var dev = Device.Get();
            dev.Compute.Set(shader.Compute);

            // src images
            for (int i = 0; i < images.NumImages; ++i)
            {
                dev.Compute.SetShaderResource(i, images.Images[i].Image.View);
            }


            for (int curMipmap = 0; curMipmap < numMipmaps; ++curMipmap)
            {
                var size = images.GetSize(curMipmap);

                // dst image
                dev.Compute.SetUnorderedAccessView(0, target.GetUaView(curMipmap));

                for (int curLayer = 0; curLayer < images.NumLayers; ++curLayer)
                {
                    constantBuffer.SetData(new InfoBuffer
                    {
                        Layer = curLayer,
                        Level = curMipmap,
                        NaNValue = float.NaN
                    });
                    dev.Compute.SetConstantBuffer(0, constantBuffer.Handle);
                    dev.Dispatch(
                        Utility.Utility.DivideRoundUp(size.Width, builder.LocalSizeX), 
                        Utility.Utility.DivideRoundUp(size.Height, builder.LocalSizeY),
                        Utility.Utility.DivideRoundUp(size.Depth, builder.LocalSizeZ));
                }
            }

            // remove images from unordered acces view slots (otherwise they can't be bound as srv later)
            dev.Compute.SetUnorderedAccessView(0, null);
        }

        private static string GetShaderSource(string colorFormula, string alphaFormula, int numImages, IShaderBuilder builder)
        {
            return
// global variables
$@"cbuffer InfoBuffer
{{
    uint layer;
    uint level;
    float NaNValue;
}};
{GetTextureBindings(numImages, builder)}
{GetTextureGetters(numImages, builder)}
{GetHelperFunctions()}
{builder.UavType} out_tex : register(u0);
[numthreads({builder.LocalSizeX}, {builder.LocalSizeY}, {builder.LocalSizeZ})]
void main(uint3 coord : SV_DISPATCHTHREADID)
{{
    uint width, height, depth, numLvl;
    texture0.GetDimensions(level, width, height, depth, numLvl);
    const float3 fcoord = (coord + 0.5) / float3(width, height, depth);
    if(!{builder.Is3DString}) coord.z = layer;

    if(coord.x >= width || coord.y >= height) return;
    if({builder.Is3DString} && coord.z >= depth) return;

    float4 color = {GetImageColor(colorFormula, alphaFormula)};
    out_tex[uint3(coord.x, coord.y, coord.z)] = color;
}}
";
        }

        private static string GetImageColor(string color, string alpha)
        {
            return $"float4(({color}).rgb, ({alpha}).a)";
        }

        private static string GetTextureBindings(int numImages, IShaderBuilder builder)
        {
            var res = "";
            for (int i = 0; i < numImages; ++i)
            {
                // binding
                res += $"{builder.SrvType} texture{i} : register(t{i});\n";
            }

            return res;
        }

        private static string GetTextureGetters(int numImages, IShaderBuilder builder)
        {
            var res = "";
            for (int i = 0; i < numImages; ++i)
            {
                res += $"float4 GetTexture{i}(uint3 pixel)" + "{\n";
                res += $"return texture{i}.mips[level][uint3(pixel.x, pixel.y, pixel.z)];" + "\n}\n";
            }

            return res;
        }

        private static string GetHelperFunctions()
        {
            return
                GetCompareFunction("fequal", "==") +
                GetCompareFunction("fbigger", ">") +
                GetCompareFunction("fsmaller", "<") +
                GetCompareFunction("fbiggereq", ">=") +
                GetCompareFunction("fsmallereq", "<=") +
                GetExtendedConstructors() +
                GetNegativeToNaNFunction("log") + 
                GetNegativeToNaNFunction("log2") + 
                GetNegativeToNaNFunction("log10") + 
                GetNegativeToNaNFunction("sqrt") + 
                GetNormalizeEx() +
                Utility.Utility.FromSrgbFunction() +
                Utility.Utility.ToSrgbFunction() +
                Utility.Utility.PowExFunction() +
                UnaryFunctionToken.GetUnaryHelperFunctions();
        }

        private static string GetCompareFunction(string name, string comparision)
        {
            return 
$@"
float4 {name}(float4 a, float4 b){{
    float4 res = float4(0.0, 0.0, 0.0, 0.0);
    [unroll]
    for(int i = 0; i < 4; ++i)
      if(a[i] {comparision} b[i])
    res[i] = 1.0;
    return res;
}}
";
        }

        private static string GetNegativeToNaNFunction(string name)
        {
            return $@"
float4 {name}Ex(float4 v) {{
    float4 r = v;
    [unroll] for(int i = 0; i < 4; ++i)
        if(r[i] < 0.0) r[i] = NaNValue;
        else r[i] = {name}(r[i]);
    return r;
}}
";
        }

        private static string GetNormalizeEx()
        {
            return $@"
float3 normalizeEx(float3 v) {{
    if(all(v == 0.0)) return NaNValue;
    return normalize(v);
}}
";
        }

        private static string GetExtendedConstructors()
        {
            return 
                "float4 f4(float v) { return float4(v, v, v, v); }\n"
                + "float3 f3(float v) { return float3(v, v, v); }\n"
                + "float3 f3(float3 v) { return v; }\n"
                + "float2 f2(float v) { return float2(v, v); }\n";
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
