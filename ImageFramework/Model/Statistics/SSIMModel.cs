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
        private ITextureCache customTexCache = null;
        private readonly Models models;
        private TransformShader lumaTransformShader = new TransformShader(TransformShader.TransformLuma, "float4", "float");
        // gauss blur as described in the paper: 11x11 window, 1.5 standard deviation
        private GaussShader gaussShader = new GaussShader(5, 1.5f * 1.5f, "float");
        private TransformShader luminanceShader = new TransformShader(new []
        {
            "in_u1", "in_u2" // expected values
        }, @"
float C1 = 0.0001; // assume dynamic range (L) of 1 => C1 = (K1*L) = (0.01 * 1)^1
float u1 = u1[coord];
float u2 = u2[coord];
return (2.0 * u1 * u2 + C1) / (u1 * u1 + u2 * u2 + C1);
", "float", "float");

        private TransformShader contrastShader = new TransformShader(new []
        {
            "in_v1", "in_v2" // variance
        }, @"
float C2 = 0.0009; // assume dynamic range (L) of 1 => C2 = (K2*L) = (0.03 * 1)^2
float v1 = in_v1[coord];
float v2 = in_v2[coord];
return (2.0 * sqrt(v1) * sqrt(v2) + C2) / (v1 + v2 + C2);
", "float", "float");

        private TransformShader structureShader = new TransformShader(new []
        {
            "in_v1", "in_v2", "in_v12"
        }, @"
float C3 = 0.009 * 0.5; // C3 = 0.5 * C2
return (in_v12[coord] + C3) / (sqrt(in_v1[coord])*sqrt(in_v2[coord]) + C3);
", "float", "float");

        private TransformShader redToRgbaTransform = new TransformShader("return float4(value, value, value, 1.0);", "float", "float4");

        public SSIMModel(Models models)
        {
            this.models = models;
        }

        private void GetLuminance(ImagesCovarianceStats src, ITexture dst, LayerMipmapSlice lm)
        {
            luminanceShader.Run(new []
            {
                src.Image1.Expected,
                src.Image2.Expected
            }, dst, lm, models.SharedModel.Upload);
        }

        /// <summary>
        /// writes the ssim luminance metric into dst image
        /// </summary>
        public void GetLumuminance(ITexture image1, ITexture image2, ITexture dst)
        {
            // get required data
        }

        private void GetContrast(ImagesCovarianceStats src, ITexture dst, LayerMipmapSlice lm)
        {
            contrastShader.Run(new []
            {
                src.Image1.Variance,
                src.Image2.Variance
            }, dst, lm, models.SharedModel.Upload);
        }

        private void GetStructure(ImagesCovarianceStats src, ITexture dst, LayerMipmapSlice lm)
        {
            structureShader.Run(new []
            {
                src.Image1.Variance,
                src.Image2.Variance,
                src.Covariance
            }, dst, lm, models.SharedModel.Upload);
        }

        private ITexture GetSSIM(ITexture luminance, ITexture contrast, ITexture structure, ITexture dst, LayerMipmapSlice lm)
        {
            Debug.Assert(luminance.HasSameDimensions(contrast));
            Debug.Assert(luminance.HasSameDimensions(structure));
            Debug.Assert(luminance.HasSameDimensions(dst));

            return null;
        }

        private ImageVarianceStats GetImageVariance(ITexture src, LayerMipmapSlice lm)
        {
            var cache = GetCache(src);

            // get luma image
            var luma = cache.GetTexture();
            lumaTransformShader.Run(src, luma, lm, models.SharedModel.Upload);

            // calc expected value from luma
            var expected = cache.GetTexture();
            gaussShader.Run(luma, expected, lm, models.SharedModel.Upload, cache);

            // calc variance with expected value and luma
            var variance = cache.GetTexture();
            // TODO

            return new ImageVarianceStats(luma, expected, variance);
        }

        private ImagesCovarianceStats GetCovariance(ImageVarianceStats image1, ImageVarianceStats image2, int layer,
            int mipmap)
        {
            var cache = GetCache(image1.Luma);

            // calc covariance
            var cov = cache.GetTexture();
            // TODO

            return new ImagesCovarianceStats(image1, image2, cov);
        }

        

        private ITextureCache GetCache(ITexture src)
        {
            // determine which texture cache to use
            ITextureCache cache = customTexCache;
            if (models.TextureCache.IsCompatibleWith(src))
            {
                cache = models.TextureCache;
            }
            else if (customTexCache == null || !customTexCache.IsCompatibleWith(src))
            {
                customTexCache?.Dispose();
                cache = customTexCache = new TextureCache(src);
            }

            return cache;
        }

        public void Dispose()
        {
            customTexCache?.Dispose();
        }
    }
}
