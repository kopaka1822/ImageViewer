using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Statistics
{
    /// <summary>
    /// contains the shader for ssim calculation
    /// </summary>
    public class SSIMModel : IDisposable
    {
        private GaussShader luminanceShader;
        private GaussShader contrastShader;
        private GaussShader structureShader;

        private ITexture GetLuminance(ITexture src, ITexture dst, int layer, int mipmap, ITextureCache cache)
        {
            Debug.Assert(src.HasSameDimensions(dst));
            Debug.Assert(cache.IsCompatibleWith(dst));

            return null;
        }

        private ITexture GetContrast(ITexture src, ITexture dst, int layer, int mipmap, ITextureCache cache)
        {
            Debug.Assert(src.HasSameDimensions(dst));
            Debug.Assert(cache.IsCompatibleWith(dst));

            return null;
        }

        private ITexture GetStructure(ITexture src, ITexture dst, int layer, int mipmap, ITextureCache cache)
        {
            Debug.Assert(src.HasSameDimensions(dst));
            Debug.Assert(cache.IsCompatibleWith(dst));

            return null;
        }

        private ITexture GetSSIM(ITexture luminance, ITexture contrast, ITexture structure, ITexture dst, int layer, int mipmap)
        {
            Debug.Assert(luminance.HasSameDimensions(contrast));
            Debug.Assert(luminance.HasSameDimensions(structure));
            Debug.Assert(luminance.HasSameDimensions(dst));

            return null;
        }

        public void Dispose()
        {

        }
    }
}
