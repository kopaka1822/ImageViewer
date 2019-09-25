using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;

namespace ImageFramework.Model.Shader
{
    public class ShaderModel : IDisposable
    {
        private QuadShader quad { get; } = new QuadShader();
        private ConvertFormatShader convert { get; } = new ConvertFormatShader();

        public ShaderModel()
        {

        }

        /// <summary>
        /// creates a new resource with the given format
        /// </summary>
        /// <param name="texture">src texture data</param>
        /// <param name="dstFormat">dst format</param>
        /// <returns></returns>
        public TextureArray2D Convert(TextureArray2D texture, ImageFormat dstFormat)
        {
            if (texture.HasFormat(dstFormat)) return texture.Clone();

            var res = new TextureArray2D(
                texture.NumLayers, texture.NumMipmaps, 
                texture.Width, texture.Height,
                dstFormat.Format, dstFormat.IsSrgb
            );

            // TODO add srgb stuff

            var dev = Device.Get();
            for (int curLayer = 0; curLayer < texture.NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < texture.NumMipmaps; ++curMipmap)
                {
                    dev.Vertex.Set(quad.Vertex);
                    dev.Pixel.Set(convert.Pixel);
                    dev.Pixel.SetShaderResource(ConvertFormatShader.InputSrvBinding, texture.GetSrView(curLayer, curMipmap));
                    dev.OutputMerger.SetRenderTargets(res.GetRtView(curLayer, curMipmap));
                    dev.SetViewScissors(texture.GetWidth(curMipmap), texture.GetHeight(curMipmap));
                    dev.DrawQuad();
                }
            }

            return res;
        }

        public void Dispose()
        {
            quad?.Dispose();
        }
    }
}
