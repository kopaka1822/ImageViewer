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
            var stats = models.GetStatistics(models.Pipelines[0].Image);

            // calculate (alpha) statistics by hand
            var cpuStats = CalcCpuStats(models.Pipelines[0].Image.GetPixelColors(0, 0));

            Assert.AreEqual(stats.Min.Alpha, cpuStats.Min);
            Assert.AreEqual(stats.Max.Alpha, cpuStats.Max);
            Assert.AreEqual(stats.Avg.Alpha, cpuStats.Avg, 0.01f);
        }

        [TestMethod]
        public void NanImage() // using an image that has nans
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "sphere_nan.dds");
            models.Apply();

            // get statistics
            var stats = models.GetStatistics(models.Pipelines[0].Image);

            // calculate (alpha) statistics by hand
            var cpuStats = CalcCpuStats(models.Pipelines[0].Image.GetPixelColors(0, 0));

            Assert.AreEqual(cpuStats.Min, stats.Min.Alpha);
            Assert.AreEqual(cpuStats.Max, stats.Max.Alpha);
            Assert.AreEqual(cpuStats.Avg, stats.Avg.Alpha, 0.01f);
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
    }
}
