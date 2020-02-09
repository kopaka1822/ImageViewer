using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            //var cpuStats = CalcCpuStats(models.Pipelines[0].Image.GetPixelColors(0, 0));

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
            var cpuStats = CalcCpuStats(models.Pipelines[0].Image.GetPixelColors(0, 0));
            
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
        }
    }
}
