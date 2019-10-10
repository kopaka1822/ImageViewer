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
        /// <param name="multiplier">rgb channels will be multiplied by this value</param>
        public TextureArray2D Convert(TextureArray2D texture, SharpDX.DXGI.Format dstFormat, int mipmap = -1,
            int layer = -1, float multiplier = 1.0f)
       {
            return Convert(texture, dstFormat, mipmap, layer, multiplier, false, 0, 0, 0, 0);
        }

        /// <summary>
        /// converts the texture into another format and performs cropping if requested
        /// </summary>
        /// <param name="texture">source texture</param>
        /// <param name="dstFormat">destination format</param>
        /// <param name="mipmap">mipmap to export, -1 for all mipmaps</param>
        /// <param name="layer">layer to export, -1 for all layers</param>
        /// <param name="multiplier">rgb channels will be multiplied by this value</param>
        /// <param name="crop">indicates if the image should be cropped, only works with 1 mipmap to export</param>
        /// <param name="xOffset">if crop: offset in source image</param>
        /// <param name="yOffset">if crop: offset in source image</param>
        /// <param name="width">if crop: width of the destination image</param>
        /// <param name="height">if crop: height of the destination image</param>
        /// <returns></returns>
        public TextureArray2D Convert(TextureArray2D texture, SharpDX.DXGI.Format dstFormat, int mipmap, int layer,
            float multiplier, bool crop, int xOffset, int yOffset, int width, int height)
        {
            Debug.Assert(ImageFormat.IsSupported(dstFormat));
            Debug.Assert(ImageFormat.IsSupported(texture.Format));

            int firstMipmap = Math.Max(mipmap, 0);
            int firstLayer = Math.Max(layer, 0);
            int nMipmaps = mipmap == -1 ? texture.NumMipmaps : 1;
            int nLayer = layer == -1 ? texture.NumLayers : 1;

            if (nMipmaps > 1 && crop)
                throw new Exception("cropping is only supported when converting a single mipmap");

            if (!crop)
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
                        Yoffset = yOffset,
                        Multiplier = multiplier
                    });

                    dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
                    dev.OutputMerger.SetRenderTargets(res.GetRtView(curLayer, curMipmap));
                    dev.SetViewScissors(res.GetWidth(curMipmap), res.GetHeight(curMipmap));
                    dev.DrawQuad();
                }
            }

            // remove bindings
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView) null);

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
    float multiplier;
};

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

float4 main(PixelIn i) : SV_TARGET
{
    float4 coord = i.projPos;
    return multiplier * in_tex.mips[level][uint3(xoffset + uint(coord.x), yoffset + uint(coord.y), layer)];
}
";
        }

        public void Dispose()
        {
            convert?.Dispose();
            quad?.Dispose();
            cbuffer?.Dispose();
        }
    }
}
