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
       
        private StatisticsShader copyToBufferShader;

        private TransformShader multiplyShader = new TransformShader(new string[]
        {
            "in_v1", "in_v2"
        }, "return in_v1[coord] * in_v2[coord];", "float", "float");

        private TransformShader subtractProductShader = new TransformShader(new string[]
        {
            "in_left", "in_right1", "in_right2"
        }, "return in_left[coord] - in_right1[coord] * in_right2[coord];", "float", "float");


        private TransformShader luminanceShader = new TransformShader(new []
        {
            "in_u1", "in_u2" // expected values
        }, @"
float C1 = 0.0001; // assume dynamic range (L) of 1 => C1 = (K1*L) = (0.01 * 1)^1
float u1 = in_u1[coord];
float u2 = in_u2[coord];
return (2.0 * u1 * u2 + C1) / (u1 * u1 + u2 * u2 + C1);
//return (2.0 * u1 * u2 + (u1 * u2 < 0.0 ? -1 : 1) * C1) / (u1 * u1 + u2 * u2 + C1);
", "float", "float");

        private TransformShader contrastShader = new TransformShader(new []
        {
            "in_v1", "in_v2" // variance
        }, @"
float C2 = 0.0009; // assume dynamic range (L) of 1 => C2 = (K2*L) = (0.03 * 1)^2
float v1 = max(in_v1[coord], 0.0);
float v2 = max(in_v2[coord], 0.0);
return (2.0 * sqrt(v1) * sqrt(v2) + C2) / (v1 + v2 + C2);
", "float", "float");

        private TransformShader structureShader = new TransformShader(new []
        {
            "in_v1", "in_v2", "in_v12"
        }, @"
float C3 = 0.0009 * 0.5; // C3 = 0.5 * C2
float v12 = in_v12[coord];
float v1 = max(in_v1[coord], 0.0);
float v2 = max(in_v2[coord], 0.0);
return (v12 + C3) / (sqrt(v1)*sqrt(v2) + C3);
//v12 can be negative => it is better to offset with the same sign
//return (v12 + (v12 < 0 ? -1 : 1) * C3) / (sqrt(v1)*sqrt(v2) + C3);
", "float", "float");
        
        private TransformShader ssimShader = new TransformShader(new []
        {
            "in_luminance", "in_structure", "in_contrast"
        }, @"
return in_luminance[coord] * in_structure[coord] * in_contrast[coord];
", "float", "float");

        private TransformShader ssimShader2 = new TransformShader(new []
        {
            "in_u1", "in_u2", "in_v1", "in_v2", "in_v12"
        }, @"
float C1 = 0.0001;
float C2 = 0.0009;
float u1 = in_u1[coord];
float u2 = in_u2[coord];
float v12 = in_v12[coord];
float v1 = max(in_v1[coord], 0.0);
float v2 = max(in_v2[coord], 0.0);
return (2*u1*u2+C1) * (2*v12+C2) / ((u1*u1+u2*u2+C1) * (v1 + v2 + C2));
//float C2Sign = v12 < 0 ? -1 : 1;
//return (2*u1*u2+C1) * (2*v12+C2Sign*C2) / ((u1*u1+u2*u2+C1) * (v1 + v2 + C2));
", "float", "float");

        private TransformShader redToRgbaTransform = new TransformShader("return float4(value, value, value, 1.0);", "float", "float4");
        private MultiscaleSSIMShader multiscale = new MultiscaleSSIMShader();

        internal void CompileShaders()
        {
            lumaTransformShader.CompileShaders();
            gaussShader.CompileShaders();
            multiplyShader.CompileShaders();
            subtractProductShader.CompileShaders();
            luminanceShader.CompileShaders();
            contrastShader.CompileShaders();
            structureShader.CompileShaders();
            ssimShader.CompileShaders();
            redToRgbaTransform.CompileShaders();
            ssimShader2.CompileShaders();
        }

        public class Settings
        {
            public bool Multiscale { get; set; } = false;
            // excludes ssim values near the image borders (5 pixels)
            public bool ExcludeBorders { get; set; } = true;
        }

        public SSIMModel(Models models)
        {
            this.models = models;
            this.copyToBufferShader = new StatisticsShader(models.SharedModel.Upload, StatisticsShader.RedValue);
        }

        /// <summary>
        /// renders the ssim luminance into a texture
        /// </summary>
        public void GetLuminanceTexture(ITexture image1, ITexture image2, ITexture dst, Settings s)
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
                    if(!s.Multiscale)
                        RenderRedToRgba(lumTex, dst, lm);
                }

                if (s.Multiscale) foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderLuminanceMultiscale(lumTex, lm);
                    RenderRedToRgba(lumTex, dst, lm);
                }
            }
            cache.StoreTexture(lumTex);
        }

        public void GetLuminanceTexture(ITexture image1, ITexture image2, ITexture dst)
         => GetLuminanceTexture(image1, image2, dst, new Settings());

        /// <summary>
        /// renders the ssim contrast into a texture
        /// </summary>
        public void GetContrastTexture(ITexture image1, ITexture image2, ITexture dst, Settings s)
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

                    if(!s.Multiscale)
                        RenderRedToRgba(contTex, dst, lm);
                }

                if (s.Multiscale) foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderContrastStructureMultiscale(contTex, lm);
                    RenderRedToRgba(contTex, dst, lm);
                }
            }
            cache.StoreTexture(contTex);
        }

        public void GetContrastTexture(ITexture image1, ITexture image2, ITexture dst)
        => GetContrastTexture(image1, image2, dst, new Settings());

        /// <summary>
        /// renders the ssim structure into a texture
        /// </summary>
        public void GetStructureTexture(ITexture image1, ITexture image2, ITexture dst, Settings s)
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
                    if(!s.Multiscale)
                        RenderRedToRgba(structTex, dst, lm);
                }

                if (s.Multiscale) foreach (var lm in image1.LayerMipmap.Range)
                {
                    RenderContrastStructureMultiscale(structTex, lm);
                    RenderRedToRgba(structTex, dst, lm);
                }
            }
            cache.StoreTexture(structTex);
        }

        public void GetStructureTexture(ITexture image1, ITexture image2, ITexture dst)
            => GetStructureTexture(image1, image2, dst, new Settings());

        public void GetSSIMTexture(ITexture image1, ITexture image2, ITexture dst, Settings s)
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
                    if (!s.Multiscale)
                    {
                        RenderSSIM(lumTex, contTex, strucTex, ssimTex, lm);
                        RenderRedToRgba(ssimTex, dst, lm);
                    }
                }

                if (s.Multiscale) foreach (var lm in image1.LayerMipmap.Range)
                {
                    // update scales
                    RenderLuminanceMultiscale(lumTex, lm);
                    RenderContrastStructureMultiscale(contTex, lm);
                    RenderContrastStructureMultiscale(strucTex, lm);
                    RenderSSIM(lumTex, contTex, strucTex, ssimTex, lm);
                    RenderRedToRgba(ssimTex, dst, lm);
                }
            }

            cache.StoreTexture(lumTex);
            cache.StoreTexture(contTex);
            cache.StoreTexture(strucTex);
            cache.StoreTexture(ssimTex);
        }

        public void GetSSIMTexture(ITexture image1, ITexture image2, ITexture dst)
        => GetSSIMTexture(image1, image2, dst, new Settings());

        public class Stats
        {
            public float Luminance;
            public float Contrast;
            public float Structure;
            public float SSIM;
            public float DSSIM => (1.0f - SSIM) / 2.0f;
        }

        public Stats GetStats(ITexture image1, ITexture image2, LayerMipmapRange lmRange, Settings s)
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
                if (!s.Multiscale)
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
                        //RenderSSIM(data, ssimTex, lm);
                        RenderSSIM(lumTex, strucTex, contTex, ssimTex, lm);
                    }
                }
                else // multiscale
                {
                    int endMipmap = Math.Min(lmRange.Mipmap + 5, image1.NumMipmaps);
                    for (int curMip = lmRange.Mipmap; curMip < endMipmap; ++curMip)
                    {
                        foreach (var lm in image1.LayerMipmap.RangeOf(new LayerMipmapRange(lmRange.Layer, curMip)))
                        {
                            // determine expected value, variance, correlation
                            RenderImagesCorrelation(image1, image2, data, lm);

                            // calc components
                            if(curMip == endMipmap - 1) // luminance only for last mipmap
                                RenderLuminance(data, lumTex, lm);
                            RenderContrast(data, contTex, lm);
                            RenderStructure(data, strucTex, lm);
                        }
                    }

                    // combine values of different scales to compute ssim
                    foreach (var lm in image1.LayerMipmap.RangeOf(lmRange))
                    {
                        // determine appropriate scale scores
                        RenderLuminanceMultiscale(lumTex, lm);
                        RenderContrastStructureMultiscale(contTex, lm);
                        RenderContrastStructureMultiscale(strucTex, lm);

                        // build ssim
                        RenderSSIM(lumTex, strucTex, contTex, ssimTex, lm);
                    }
                }
            }

            var stats = new Stats
            {
                Luminance = GetAveragedValue(lumTex, lmRange, s.ExcludeBorders),
                Contrast = GetAveragedValue(contTex, lmRange, s.ExcludeBorders),
                Structure = GetAveragedValue(strucTex, lmRange, s.ExcludeBorders),
                SSIM = GetAveragedValue(ssimTex, lmRange, s.ExcludeBorders)
            };

            cache.StoreTexture(lumTex);
            cache.StoreTexture(contTex);
            cache.StoreTexture(strucTex);
            cache.StoreTexture(ssimTex);

            return stats;
        }

        public Stats GetStats(ITexture image1, ITexture image2, LayerMipmapRange lmRange)
            => GetStats(image1, image2, lmRange, new Settings());

        private float GetAveragedValue(ITexture tex, LayerMipmapRange lm, bool noBorders)
        {
            var offset = noBorders ? new Size3(5) : Size3.Zero; // don't get values from blur borders

            // obtain gpu buffer that is big enough to hold all elements
            var numElements = copyToBufferShader.GetRequiredElementCount(tex, lm, offset);

            var buffer = models.Stats.GetBuffer(numElements);
            var reduce = models.Stats.AvgReduce;

            // copy values into buffer for scan
            copyToBufferShader.CopyToBuffer(tex, buffer, lm, offset);
            reduce.Run(buffer, numElements);
            models.SharedModel.Download.CopyFrom(buffer, sizeof(float));
            return models.SharedModel.Download.GetData<float>() / numElements;
        }

        private void RenderLuminance(ImagesCorrelationStats src, ITexture dst, LayerMipmapSlice lm)
        {
            luminanceShader.Run(new []
            {
                src.Image1.Expected,
                src.Image2.Expected
            }, dst, lm, models.SharedModel.Upload);
        }

        private void RenderLuminanceMultiscale(ITexture texture, LayerMipmapSlice lm)
        {
            multiscale.RunCopy(texture, lm, models.SharedModel.Upload);
        }

        private void RenderContrast(ImagesCorrelationStats src, ITexture dst, LayerMipmapSlice lm)
        {
            contrastShader.Run(new []
            {
                src.Image1.Variance,
                src.Image2.Variance
            }, dst, lm, models.SharedModel.Upload);
        }

        private void RenderContrastStructureMultiscale(ITexture texture, LayerMipmapSlice lm)
        {
            multiscale.RunWeighted(texture, lm, models.SharedModel.Upload);
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

        private void RenderSSIM(ImagesCorrelationStats src, ITexture dst, LayerMipmapSlice lm)
        {
            ssimShader2.Run(new []{src.Image1.Expected, src.Image2.Expected, src.Image1.Variance, src.Image2.Variance, src.Correlation}, dst, lm, models.SharedModel.Upload);
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
            gaussShader.Run(dst.Luma, dst.Expected, lm, models.SharedModel.Upload, dst.Cache);

            // calculate luma squared
            var lumaSq = dst.Cache.GetTexture();
            multiplyShader.Run(new []{dst.Luma, dst.Luma}, lumaSq, lm, models.SharedModel.Upload);
            // blur luma squared
            var lumaBlur = dst.Cache.GetTexture();
            gaussShader.Run(lumaSq, lumaBlur, lm, models.SharedModel.Upload, dst.Cache);
            
            // calc variance with: blurred(luma^2) - mu^2
            subtractProductShader.Run(new []{lumaBlur, dst.Expected, dst.Expected}, dst.Variance, lm, models.SharedModel.Upload);

            dst.Cache.StoreTexture(lumaSq);
            dst.Cache.StoreTexture(lumaBlur);
        }

        private void RenderImagesCorrelation(ITexture src1, ITexture src2, ImagesCorrelationStats dst,
            LayerMipmapSlice lm)
        {
            // calc expected value and variance
            RenderImageVariance(src1, dst.Image1, lm);
            RenderImageVariance(src2, dst.Image2, lm);
            // calc correlation coefficient
            var lumaMult = dst.Cache.GetTexture();
            multiplyShader.Run(new []{dst.Image1.Luma, dst.Image2.Luma}, lumaMult, lm, models.SharedModel.Upload);
            // blur result
            var lumaBlur = dst.Cache.GetTexture();
            gaussShader.Run(lumaMult, lumaBlur, lm, models.SharedModel.Upload, dst.Cache);

            // calc correlation with blurred(luma1*luma2) - mu1*mu2
            subtractProductShader.Run(new []{lumaBlur, dst.Image1.Expected, dst.Image2.Expected}, dst.Correlation, lm, models.SharedModel.Upload);

            dst.Cache.StoreTexture(lumaMult);
            dst.Cache.StoreTexture(lumaBlur);
        }

        private ITextureCache GetCache(ITexture src)
        {
            // determine which texture cache to use
            ITextureCache cache = customTexCache;
            if (cache == null || !cache.IsCompatibleWith(src))
            {
                customTexCache?.Dispose();
                cache = customTexCache = new TextureCache(src, Format.R32_Float, true);
            }

            return cache;
        }

        public void Dispose()
        {
            customTexCache?.Dispose();
        }
    }
}
