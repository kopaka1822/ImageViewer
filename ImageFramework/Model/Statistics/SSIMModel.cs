using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.SqlServer.Server;
using Format = SharpDX.DXGI.Format;

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
        private VarianceShader varianceShader = new VarianceShader(5, 1.5f * 1.5f);
        private CorrelationCoefficientShader cocoefShader = new CorrelationCoefficientShader(5, 1.5f * 1.5f);

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
        
        private TransformShader ssimShader = new TransformShader(new []
        {
            "in_luminance", "in_structure", "in_contrast"
        }, @"
return in_luminance[coord] * in_structure[coord] * in_contrast[coord];
", "float", "float");

        private TransformShader redToRgbaTransform = new TransformShader("return float4(value, value, value, 1.0);", "float", "float4");
        

        public SSIMModel(Models models)
        {
            this.models = models;
        }

        /// <summary>
        /// renders the ssim luminance into a texture
        /// </summary>
        public void GetLuminanceTexture(ITexture image1, ITexture image2, ITexture dst)
        {
            Debug.Assert(image1.HasSameDimensions(image2));
            Debug.Assert(image1.HasSameDimensions(dst));

            var cache = GetCache(image1);
            var lumTex = cache.GetTexture();
            using (var data = new ImagesCorrelationStats(cache))
            {
                foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderImagesCorrelation(image1, image2, data, lm);
                    RenderLuminance(data, lumTex, lm);
                    RenderRedToRgba(lumTex, dst, lm);
                }
            }
            cache.StoreTexture(lumTex);
        }

        /// <summary>
        /// renders the ssim contrast into a texture
        /// </summary>
        public void GetContrastTexture(ITexture image1, ITexture image2, ITexture dst)
        {
            Debug.Assert(image1.HasSameDimensions(image2));
            Debug.Assert(image1.HasSameDimensions(dst));

            var cache = GetCache(image1);
            var contTex = cache.GetTexture();
            using (var data = new ImagesCorrelationStats(cache))
            {
                foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderImagesCorrelation(image1, image2, data, lm);
                    RenderContrast(data, contTex, lm);
                    RenderRedToRgba(contTex, dst, lm);
                }
            }
            cache.StoreTexture(contTex);
        }

        /// <summary>
        /// renders the ssim structure into a texture
        /// </summary>
        public void GetStructureTexture(ITexture image1, ITexture image2, ITexture dst)
        {
            Debug.Assert(image1.HasSameDimensions(image2));
            Debug.Assert(image1.HasSameDimensions(dst));

            var cache = GetCache(image1);
            var structTex = cache.GetTexture();
            using (var data = new ImagesCorrelationStats(cache))
            {
                foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderImagesCorrelation(image1, image2, data, lm);
                    RenderStructure(data, structTex, lm);
                    RenderRedToRgba(structTex, dst, lm);
                }
            }
            cache.StoreTexture(structTex);
        }

        public void GetSSIMTexture(ITexture image1, ITexture image2, ITexture dst)
        {
            Debug.Assert(image1.HasSameDimensions(image2));
            Debug.Assert(image1.HasSameDimensions(dst));

            var cache = GetCache(image1);
            var lumTex = cache.GetTexture();
            var strucTex = cache.GetTexture();
            var contTex = cache.GetTexture();
            var ssimTex = cache.GetTexture();
            using (var data = new ImagesCorrelationStats(cache))
            {
                foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderImagesCorrelation(image1, image2, data, lm);
                    RenderLuminance(data, lumTex, lm);
                    RenderContrast(data, contTex, lm);
                    RenderStructure(data, strucTex, lm);
                    RenderSSIM(lumTex, contTex, strucTex, ssimTex, lm);
                    RenderRedToRgba(ssimTex, dst, lm);
                }
            }

            cache.StoreTexture(lumTex);
            cache.StoreTexture(contTex);
            cache.StoreTexture(strucTex);
            cache.StoreTexture(ssimTex);
        }

        public class Stats
        {
            public float Luminance;
            public float Contrast;
            public float Structure;
            public float SSIM;
        }

        public Stats GetStats(ITexture image1, ITexture image2, LayerMipmapRange lmRange)
        {
            Debug.Assert(image1.HasSameDimensions(image2));
            Debug.Assert(lmRange.IsSingleMipmap);
            var cache = GetCache(image1);

            var lumTex = cache.GetTexture();
            var contTex = cache.GetTexture();
            var strucTex = cache.GetTexture();
            var ssimTex = cache.GetTexture();
            using (var data = new ImagesCorrelationStats(cache))
            {
                foreach (var lm in image1.LayerMipmap.RangeOf(lmRange))
                {
                    // determine expected value, variance, correlation
                    RenderImagesCorrelation(image1, image2, data, lm);

                    // calc the three components
                    RenderLuminance(data, lumTex, lm);
                    RenderContrast(data, contTex, lm);
                    RenderStructure(data, strucTex, lm);

                    // build ssim
                    RenderSSIM(lumTex, strucTex, contTex, ssimTex, lm);
                }
            }

            // TODO put data into a buffer and read result


            cache.StoreTexture(lumTex);
            cache.StoreTexture(contTex);
            cache.StoreTexture(strucTex);
            cache.StoreTexture(ssimTex);

            return new Stats
            {
                Luminance = 1.0f,
                Contrast = 1.0f,
                Structure = 1.0f,
                SSIM = 1.0f
            };
        }

        private void RenderLuminance(ImagesCorrelationStats src, ITexture dst, LayerMipmapSlice lm)
        {
            luminanceShader.Run(new []
            {
                src.Image1.Expected,
                src.Image2.Expected
            }, dst, lm, models.SharedModel.Upload);
        }

        private void RenderContrast(ImagesCorrelationStats src, ITexture dst, LayerMipmapSlice lm)
        {
            contrastShader.Run(new []
            {
                src.Image1.Variance,
                src.Image2.Variance
            }, dst, lm, models.SharedModel.Upload);
        }

        private void RenderStructure(ImagesCorrelationStats src, ITexture dst, LayerMipmapSlice lm)
        {
            structureShader.Run(new []
            {
                src.Image1.Variance,
                src.Image2.Variance,
                src.Correlation
            }, dst, lm, models.SharedModel.Upload);
        }

        private void RenderSSIM(ITexture luminance, ITexture contrast, ITexture structure, ITexture dst, LayerMipmapSlice lm)
        {
            Debug.Assert(luminance.HasSameDimensions(contrast));
            Debug.Assert(luminance.HasSameDimensions(structure));
            Debug.Assert(luminance.HasSameDimensions(dst));

            ssimShader.Run(new[] { luminance, contrast, structure }, dst, lm, models.SharedModel.Upload);
        }

        private void RenderRedToRgba(ITexture src, ITexture dst, LayerMipmapSlice lm)
        {
            Debug.Assert(src.HasSameDimensions(dst));
            Debug.Assert(src.Format == Format.R32_Float);
            Debug.Assert(dst.Format == Format.R32G32B32A32_Float);
            redToRgbaTransform.Run(src, dst, lm, models.SharedModel.Upload);
        }

        private void RenderImageVariance(ITexture src, ImageVarianceStats dst, LayerMipmapSlice lm)
        {
            // luma values
            lumaTransformShader.Run(src, dst.Luma, lm, models.SharedModel.Upload);
            // expected value
            gaussShader.Run(src, dst.Expected, lm, models.SharedModel.Upload, dst.Cache);
            // variance
            varianceShader.Run(src, dst.Expected, dst.Variance, lm, models.SharedModel.Upload);
        }

        private void RenderImagesCorrelation(ITexture src1, ITexture src2, ImagesCorrelationStats dst,
            LayerMipmapSlice lm)
        {
            // calc expected value and variance
            RenderImageVariance(src1, dst.Image1, lm);
            RenderImageVariance(src2, dst.Image2, lm);
            // calc correlation coefficient
            cocoefShader.Run(dst.Correlation, dst.Image1.Luma, dst.Image2.Luma, dst.Image1.Expected, dst.Image2.Expected, lm, models.SharedModel.Upload);
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
