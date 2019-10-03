using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Filter
{
    [TestClass]
    public class FilterLoaderTest
    {
        [TestMethod]
        public void AlphaBackground()
        {
            TestFilter("alpha_background.hlsl");
        }

        [TestMethod]
        public void Blur()
        {
            TestFilter("blur.hlsl");
        }

        [TestMethod]
        public void Clamp()
        {
            TestFilter("clamp.hlsl");
        }

        [TestMethod]
        public void Denoise()
        {
            TestFilter("denoise.hlsl");
        }

        [TestMethod]
        public void Enhance()
        {
            TestFilter("enhance.hlsl");
        }

        [TestMethod]
        public void Gamma()
        {
            TestFilter("gamma.hlsl");
        }

        [TestMethod]
        public void Heatmap()
        {
            TestFilter("heatmap.hlsl");
        }

        [TestMethod]
        public void Highlight()
        {
            TestFilter("highlight.hlsl");
        }

        [TestMethod]
        public void Luminance()
        {
            TestFilter("luminance.hlsl");
        }

        [TestMethod]
        public void Median()
        {
            TestFilter("median.hlsl");
        }

        [TestMethod]
        public void Mirror()
        {
            TestFilter("mirror.hlsl");
        }

        [TestMethod]
        public void Quantile()
        {
            TestFilter("quantile.hlsl");
        }

        [TestMethod]
        public void SetColor()
        {
            TestFilter("set_color.hlsl");
        }

        [TestMethod]
        public void Silhouette()
        {
            TestFilter("silhouette.hlsl");
        }

        private void TestFilter(string name)
        {
            var loader = new FilterLoader("filter/" + name);

            var test = new FilterModel(loader, 1);
        }
    }
}
