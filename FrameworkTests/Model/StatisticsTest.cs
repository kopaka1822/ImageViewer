using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.Model.Shader;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model
{
    [TestClass]
    public class StatisticsTest
    {
        private struct AlphaStats
        {
            public float Min;
            public float Max;
            public float Avg;
        }

        [TestMethod]
        public void Checkers()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers.dds");
            models.Apply();

            // get statistics
            var stats = models.Stats.GetStatisticsFor(models.Pipelines[0].Image);

            // calculate (alpha) statistics by hand
            //var cpuStats = CalcCpuStats(models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Mip0));

            // alpha
            Assert.AreEqual(1.0f, stats.Alpha.Min);
            Assert.AreEqual(1.0f, stats.Alpha.Max);
            Assert.AreEqual(1.0f, stats.Alpha.Avg, 0.01f);

            // luminance
            Assert.AreEqual(0.0f, stats.Luminance.Min);
            Assert.AreEqual(1.0f, stats.Luminance.Max, 0.01f);
            Assert.AreEqual(0.5f, stats.Luminance.Avg, 0.01f);

            // luma
            Assert.AreEqual(0.0f, stats.Luma.Min);
            Assert.AreEqual(1.0f, stats.Luma.Max, 0.01f);
            Assert.AreEqual(0.5f, stats.Luma.Avg, 0.01f);

            // average
            Assert.AreEqual(0.0f, stats.Average.Min);
            Assert.AreEqual(1.0f, stats.Average.Max, 0.01f);
            Assert.AreEqual(0.5f, stats.Average.Avg, 0.01f);

            // lightness
            Assert.AreEqual(0.0f, stats.Lightness.Min);
            Assert.AreEqual(100.0f, stats.Lightness.Max, 0.5f);
            Assert.AreEqual(50.0f, stats.Lightness.Avg, 0.5f);
        }

        [TestMethod]
        public void Checkers3D()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers3d.dds");
            models.Apply();

            // get statistics
            var stats = models.Stats.GetStatisticsFor(models.Pipelines[0].Image);

            // alpha
            Assert.AreEqual(1.0f, stats.Alpha.Min);
            Assert.AreEqual(1.0f, stats.Alpha.Max);
            Assert.AreEqual(1.0f, stats.Alpha.Avg, 0.01f);     

            // luminance
            Assert.AreEqual(0.0f, stats.Luminance.Min);
            Assert.AreEqual(1.0f, stats.Luminance.Max, 0.01f);     
            Assert.AreEqual(0.5f, stats.Luminance.Avg, 0.01f);     

            // luma
            Assert.AreEqual(0.0f, stats.Luma.Min);
            Assert.AreEqual(1.0f, stats.Luma.Max, 0.01f);
            Assert.AreEqual(0.5f, stats.Luma.Avg, 0.01f);

            // average
            Assert.AreEqual(0.0f, stats.Average.Min);
            Assert.AreEqual(1.0f, stats.Average.Max, 0.01f);
            Assert.AreEqual(0.5f, stats.Average.Avg, 0.01f);

            // lightness
            Assert.AreEqual(0.0f, stats.Lightness.Min);
            Assert.AreEqual(100.0f, stats.Lightness.Max, 0.5f);
            Assert.AreEqual(50.0f, stats.Lightness.Avg, 0.5f);
        }

        [TestMethod]
        public void NanImage() // using an image that has nans
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "sphere_nan.dds");
            models.Apply();

            // get statistics
            var stats = models.Stats.GetStatisticsFor(models.Pipelines[0].Image);

            // calculate (alpha) statistics by hand
            var cpuStats = CalcCpuStats(models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Mip0));
            
            Assert.AreEqual(cpuStats.Min, stats.Alpha.Min);
            Assert.AreEqual(cpuStats.Max, stats.Alpha.Max);
            Assert.AreEqual(cpuStats.Avg, stats.Alpha.Avg, 0.01f);
        }

        private AlphaStats CalcCpuStats(Color[] colors)
        {
            var min = float.MaxValue;
            var max = float.MinValue;
            var sum = 0.0f;
            foreach (var c in colors)
            {
                float col = c.Alpha;
                if (float.IsNaN(col))
                {
                    col = 0.0f;
                }

                min = Math.Min(min, col);
                max = Math.Max(max, col);
                sum += col;
            }

            var avg = sum / colors.Length;

            return new AlphaStats
            {
                Avg = avg,
                Max = max,
                Min = min
            };
        }

        [TestMethod]
        public void SSIMCompile()
        {
            var models = new Models(1);
            var s = models.SSIM;
            s.CompileShaders();
        }

        [TestMethod]
        public void SSIMSphereIdentical()
        {
            var models = new Models(1);
            var sphere1 = IO.LoadImageTexture(TestData.Directory + "sphere.png");
            var sphere2 = IO.LoadImageTexture(TestData.Directory + "sphere.png");

            var stats = models.SSIM.GetStats(sphere1, sphere2, LayerMipmapRange.MostDetailed, new SSIMModel.Settings
            {
                ExcludeBorders = false
            });
            Assert.AreEqual(1.0f, stats.Luminance, 0.01f);
            Assert.AreEqual(1.0f, stats.Structure, 0.01f);
            Assert.AreEqual(1.0f, stats.Contrast, 0.01f);
            Assert.AreEqual(1.0f, stats.SSIM, 0.01f);
            Assert.AreEqual(0.0f, stats.DSSIM, 0.01f);
        }

        [TestMethod]
        public void SSIMSphereTest()
        {
            var models = new Models(1);
            var sphere = IO.LoadImageTexture(TestData.Directory + "sphere.png");
            var sphereMedian = IO.LoadImageTexture(TestData.Directory + "sphere_median.png");
            var sphereBlur = IO.LoadImageTexture(TestData.Directory + "sphere_blur.png");

            var settings = new SSIMModel.Settings
            {
                ExcludeBorders = false
            };

            var stats = models.SSIM.GetStats(sphere, sphereMedian, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.9912f, stats.SSIM, 0.01f);

            stats = models.SSIM.GetStats(sphere, sphereBlur, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.320421f, stats.SSIM, 0.01f);
        }


        [TestMethod]
        public void SSIMEinsteinTest()
        {
            var models = new Models(1);
            var einstein = IO.LoadImageTexture(TestData.Directory + "einstein/ref.jpg");
            var einstein0662 = IO.LoadImageTexture(TestData.Directory + "einstein/ssim0662.jpg");
            var einstein0694 = IO.LoadImageTexture(TestData.Directory + "einstein/ssim0694.jpg");
            var einstein0840 = IO.LoadImageTexture(TestData.Directory + "einstein/ssim0840.jpg");
            var einstein0913 = IO.LoadImageTexture(TestData.Directory + "einstein/ssim0913.jpg");
            var einstein0988 = IO.LoadImageTexture(TestData.Directory + "einstein/ssim0988.jpg");

            var settings = new SSIMModel.Settings
            {
                ExcludeBorders = true
            };

            // reference values are taken from the authors matlab implementation

            var stats = models.SSIM.GetStats(einstein, einstein0662, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.71071f, stats.SSIM, 0.01f);

            stats = models.SSIM.GetStats(einstein, einstein0694, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.73680f, stats.SSIM, 0.01f);

            stats = models.SSIM.GetStats(einstein, einstein0840, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.86391f, stats.SSIM, 0.01f);

            stats = models.SSIM.GetStats(einstein, einstein0913, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.88293f, stats.SSIM, 0.01f);

            stats = models.SSIM.GetStats(einstein, einstein0988, LayerMipmapRange.MostDetailed, settings);
            Assert.AreEqual(0.98780f, stats.SSIM, 0.01f);
        }

        [TestMethod]
        public void SSIMMultiscaleCompile()
        {
            var s = new MultiscaleSSIMShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void SSIMMultiscaleLuminance()
        {
            var s = new MultiscaleSSIMShader();
            var toRed = new ImageFramework.Model.Shader.TransformShader("return value.r;", "float4", "float");
            var fromRed = new ImageFramework.Model.Shader.TransformShader("return float4(value, value, value, 1.0);", "float", "float4");
            var tex = IO.LoadImageTexture(TestData.Directory + "checkers.dds");
            var redTex = new TextureArray2D(tex.LayerMipmap, tex.Size, Format.R32_Float, true);
            var dstTex = new TextureArray2D(tex.LayerMipmap, tex.Size, Format.R32G32B32A32_Float, true);
            var upload = new UploadBuffer(256);

            Assert.AreEqual(tex.NumMipmaps, 3);
            var expected = tex.GetPixelColors(LayerMipmapSlice.Mip2)[0];

            // copy checkers red channel only
            foreach (var lm in tex.LayerMipmap.Range)
            {
                toRed.Run(tex, redTex, lm, upload);
            }

            // this should copy lowest resolution mipmap to first mipmap
            s.RunCopy(redTex, LayerMipmapSlice.Mip0, upload);

            // copy back
            foreach (var lm in tex.LayerMipmap.Range)
            {
                fromRed.Run(redTex, dstTex, lm, upload);
            }

            var actual = dstTex.GetPixelColors(LayerMipmapSlice.Mip0);
            foreach (var color in actual)
            {
                Assert.IsTrue(color.Equals(expected, Color.Channel.R, 0.0011f));
            }
        }
    }
}
