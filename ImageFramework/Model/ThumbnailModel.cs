using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model
{
    /// <summary>
    /// responsible for creating thumbnails
    /// </summary>
    public class ThumbnailModel : IDisposable
    {
        private readonly QuadShader quad;
        private readonly DirectX.Shader convert;
        private readonly SamplerState sampler;

        public ThumbnailModel()
        {
            quad = new QuadShader();
            convert = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "ThumbnailPixelShader");
            var samplerDesc = new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = new RawColor4(),
                ComparisonFunction = Comparison.Never,
                Filter = SharpDX.Direct3D11.Filter.MinLinearMagMipPoint,
                MaximumAnisotropy = 1,
                MaximumLod = float.MaxValue,
                MinimumLod = 0,
                MipLodBias = 0
            };
            sampler = new SamplerState(Device.Get().Handle, samplerDesc);
        }

        /// <summary>
        /// creates a thumbnail for one image layer/mipmap
        /// </summary>
        /// <param name="size">maximum width/height of the thumbnail</param>
        /// <param name="texture">source texture</param>
        /// <param name="dstFormat">destination texture format</param>
        /// <param name="layer">source layer</param>
        /// <param name="mipmap">source mipmap</param>
        /// <returns>texture with width, height smaller or equal to size. One layer and one mipmap</returns>
        public TextureArray2D CreateThumbnail(int size, TextureArray2D texture,
            SharpDX.DXGI.Format dstFormat = Format.R8G8B8A8_UNorm_SRgb, int layer = 0, int mipmap = 0)
        {
            Debug.Assert(ImageFormat.IsSupported(dstFormat));
            Debug.Assert(ImageFormat.IsSupported(texture.Format));

            // determine dimensions of output texture
            var width = 0;
            var height = 0;
            if (texture.Width > texture.Height)
            {
                width = size;
                height = (texture.Height * size) / texture.Width;
            }
            else
            {
                height = size;
                width = (texture.Width * size) / texture.Height;
            }
            Debug.Assert(width <= size);
            Debug.Assert(height <= size);

            var res = new TextureArray2D(1, 1, width, height, dstFormat, false);

            var dev = Device.Get();
            dev.Vertex.Set(quad.Vertex);
            dev.Pixel.Set(convert.Pixel);
            // TODO compute which mipmap has the closest fit
            dev.Pixel.SetShaderResource(0, texture.GetSrView(layer, mipmap));
            dev.Pixel.SetSampler(0, sampler);

            dev.OutputMerger.SetRenderTargets(res.GetRtView(0, 0));
            dev.SetViewScissors(width, height);
            dev.DrawQuad();

            // rempve bindings
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);

            return res;
        }

        private static string GetSource()
        {
            return @"
Texture2D<float4> tex : register(t0);
SamplerState texSampler : register(s0);

struct PixelIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

float4 main(PixelIn i) : SV_TARGET
{
    float4 color = tex.Sample(texSampler, i.texcoord);
    return color;
}
";
        }

        public void Dispose()
        {
            quad?.Dispose();
            convert?.Dispose();
            sampler?.Dispose();
        }
    }
}
