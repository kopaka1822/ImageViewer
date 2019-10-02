using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.ImageLoader;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace ImageFramework.Model.Shader
{
    public class ConvertFormatShader : IDisposable
    {
        private readonly DirectX.Shader convert;
        private readonly QuadShader quad;
        private readonly UploadBuffer<LayerLevelOffsetData> cbuffer;
        public ConvertFormatShader()
        {
            var dev = DirectX.Device.Get();
            convert = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "ConvertFormatShader");
            quad = new QuadShader();
            cbuffer = new UploadBuffer<LayerLevelOffsetData>(1);
        }

        /// <summary>
        /// converts the texture into another format and performs cropping if requested
        /// </summary>
        /// <param name="texture">source texture</param>
        /// <param name="dstFormat">destination format</param>
        /// <param name="mipmap">mipmap to export, -1 for all mipmaps</param>
        /// <param name="layer">layer to export, -1 for all layers</param>
        public TextureArray2D Convert(TextureArray2D texture, SharpDX.DXGI.Format dstFormat, int mipmap = -1, int layer = -1)
        {
            return Convert(texture, dstFormat, mipmap, layer, false, 0, 0, 0, 0);
        }

        /// <summary>
        /// converts the texture into another format and performs cropping if requested
        /// </summary>
        /// <param name="texture">source texture</param>
        /// <param name="dstFormat">destination format</param>
        /// <param name="mipmap">mipmap to export, -1 for all mipmaps</param>
        /// <param name="layer">layer to export, -1 for all layers</param>
        /// <param name="crop">indicates if the image should be cropped, only works with 1 mipmap to export</param>
        /// <param name="xOffset">if crop: offset in source image</param>
        /// <param name="yOffset">if crop: offset in source image</param>
        /// <param name="width">if crop: width of the destination image</param>
        /// <param name="height">if crop: height of the destination image</param>
        /// <returns></returns>
        public TextureArray2D Convert(TextureArray2D texture, SharpDX.DXGI.Format dstFormat, int mipmap, int layer, bool crop, int xOffset, int yOffset, int width, int height)
        {
            Debug.Assert(ImageFormat.IsSupported(dstFormat));
            Debug.Assert(ImageFormat.IsSupported(texture.Format));

            int firstMipmap = Math.Max(mipmap, 0);
            int firstLayer = Math.Max(layer, 0);
            int nMipmaps = mipmap == -1 ? texture.NumMipmaps : 1;
            int nLayer = layer == -1 ? texture.NumLayers : 1;

            if(nMipmaps > 1 && crop)
                throw new Exception("cropping is only supported when converting a single mipmap");

            if(!crop)
            {
                width = texture.GetWidth(firstMipmap);
                height = texture.GetHeight(firstMipmap);
                xOffset = 0;
                yOffset = 0;
            }

            var res = new TextureArray2D(nLayer, nMipmaps, width, height, dstFormat, false);

            var dev = DirectX.Device.Get();
            dev.Vertex.Set(quad.Vertex);
            dev.Pixel.Set(convert.Pixel);

            dev.Pixel.SetShaderResource(0, texture.View);

            for (int curLayer = 0; curLayer < nLayer; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
                {
                    cbuffer.SetData(new LayerLevelOffsetData
                    {
                        Layer = curLayer + firstLayer,
                        Level = curMipmap + firstMipmap,
                        Xoffset = xOffset,
                        Yoffset = yOffset
                    });

                    dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
                    dev.OutputMerger.SetRenderTargets(res.GetRtView(curLayer, curMipmap));
                    dev.SetViewScissors(res.GetWidth(curMipmap), res.GetHeight(curMipmap));
                    dev.DrawQuad();
                }
            }

            return res;
        }

        private static string GetSource()
        {
            return @"
Texture2DArray<float4> in_tex : register(t0);

cbuffer InfoBuffer : register(b0)
{
    uint layer;
    uint level;
    uint xoffset;
    uint yoffset;
};

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

float4 main(PixelIn i) : SV_TARGET
{
    float4 coord = i.projPos;
    return in_tex.mips[level][uint3(xoffset + uint(coord.x), yoffset + uint(coord.y), layer)];
}
";
        }

        public void Dispose()
        {
            convert?.Dispose();
            quad?.Dispose();
            cbuffer?.Dispose();
        }

        public static string FromSrgbFunction()
        {
            return
                @"float4 fromSrgb(float4 c){
    float3 r;
    [unroll]
    for(int i = 0; i < 3; ++i){
        if(c[i] > 1.0) r[i] = 1.0;
        else if(c[i] < 0.0) r[i] = 0.0;
        else if(c[i] <= 0.04045) r[i] = c[i] / 12.92;
        else r[i] = pow((c[i] + 0.055)/1.055, 2.4);
    }
    return float4(r, c.a);
}";
        }

        public static string ToSrgbFunction()
        {
            return
                @"float4 toSrgb(float4 c){
    float3 r;
    [unroll]
    for(int i = 0; i < 3; ++i){
        if( c[i] > 1.0) r[i] = 1.0;
        else if( c[i] < 0.0) r[i] = 0.0;
        else if( c[i] <= 0.0031308) r[i] = 12.92 * c[i];
        else r[i] = 1.055 * pow(c[i], 0.41666) - 0.055;
    }
    return float4(r, c.a);
}";
        }
    }
}
