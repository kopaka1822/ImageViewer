using System;
using System.Security.Cryptography;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;

namespace ImageFramework.Model.Shader
{
    internal class ImageCombineShader : IDisposable
    {
        private const int LocalSize = 8;

        private DirectX.Shader shader;

        public ImageCombineShader(string colorFormula, string alphaFormula, int numImages)
        {
            shader = new DirectX.Shader(DirectX.Shader.Type.Compute,
                GetShaderSource(colorFormula, alphaFormula, Math.Max(numImages, 1)), "ImageCombineShader");
        }

        public void Run(ImagesModel images, UploadBuffer<LayerLevelFilter> constantBuffer, TextureArray2D target)
        {
            var dev = Device.Get();
            dev.Compute.Set(shader.Compute);

            // src images
            for (int i = 0; i < images.NumImages; ++i)
            {
                dev.Compute.SetShaderResource(i, images.Images[i].Image.View);
            }

            for (int curMipmap = 0; curMipmap < images.NumMipmaps; ++curMipmap)
            {
                var width = images.GetWidth(curMipmap);
                var height = images.GetHeight(curMipmap);

                // dst image
                dev.Compute.SetUnorderedAccessView(0, target.GetUaView(curMipmap));

                for (int curLayer = 0; curLayer < images.NumLayers; ++curLayer)
                {
                    constantBuffer.SetData(new LayerLevelFilter{Layer = curLayer, Level = curMipmap});
                    dev.Compute.SetConstantBuffer(0, constantBuffer.Handle);
                    dev.Dispatch(Utility.Utility.DivideRoundUp(width, LocalSize), Utility.Utility.DivideRoundUp(height, LocalSize));
                }
            }

            // remove images from unordered acces view slots (otherwise they can't be bound as srv later)
            dev.Compute.SetUnorderedAccessView(0, null);
        }

        private static string GetShaderSource(string colorFormula, string alphaFormula, int numImages)
        {
            return
// global variables
@"cbuffer InfoBuffer
{
    uint layer;
    uint level;
};
" + GetTextureBindings(numImages) +
GetTextureGetters(numImages) +
GetHelperFunctions() +
ConvertFormatShader.FromSrgbFunction() +
ConvertFormatShader.ToSrgbFunction() +
"\nRWTexture2DArray<float4> out_tex : register(u0);\n" +
$"[numthreads({LocalSize},{LocalSize}, 1)]" +
$@"
void main(uint3 coord : SV_DISPATCHTHREADID)
{{
    uint width, height, elements, numLvl;
    texture0.GetDimensions(level, width, height, elements, numLvl);
    if(coord.x >= width || coord.y >= height) return;
    float4 color = {GetImageColor(colorFormula, alphaFormula)};
    out_tex[uint3(coord.x, coord.y, layer)] = color;
}}
";
        }

        private static string GetImageColor(string color, string alpha)
        {
            return $"float4(({color}).rgb, ({alpha}).a)";
        }

        private static string GetTextureBindings(int numImages)
        {
            var res = "";
            for (int i = 0; i < numImages; ++i)
            {
                // binding
                res += $"Texture2DArray<float4> texture{i} : register(t{i});\n";
            }

            return res;
        }

        private static string GetTextureGetters(int numImages)
        {
            var res = "";
            for (int i = 0; i < numImages; ++i)
            {
                res += $"float4 GetTexture{i}(uint2 pixel)" + "{\n";
                res += $"return texture{i}.mips[level][uint3(pixel.x, pixel.y, layer)];" + "\n}\n";
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
                GetExtendedConstructors();
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
