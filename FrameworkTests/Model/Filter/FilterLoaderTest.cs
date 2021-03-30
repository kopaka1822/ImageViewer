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
            TestFilter("denoise.hlsl", FilterLoader.TargetType.Tex2D);
        }

        [TestMethod]
        public void Enhance()
        {
            TestFilter("enhance.hlsl", FilterLoader.TargetType.Tex2D);
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
            TestFilter("quantile.hlsl", FilterLoader.TargetType.Tex2D);
        }

        [TestMethod]
        public void Silhouette()
        {
            TestFilter("silhouette.hlsl", FilterLoader.TargetType.Tex2D);
        }

        [TestMethod]
        public void HeightToNormal()
        {
            TestFilter("heightToNormal.hlsl", FilterLoader.TargetType.Tex2D);
        }

        [TestMethod]
        public void Bilateral()
        {
            TestFilter("bilateral.hlsl", FilterLoader.TargetType.Tex2D);
        }

        [TestMethod]
        public void GuidedBilateral()
        {
            TestFilter("guided_bilateral.hlsl", FilterLoader.TargetType.Tex2D);
        }

        [TestMethod]
        public void Divergent()
        {
            TestFilter("divergent.hlsl");
        }

        [TestMethod]
        public void Dithering()
        {
            TestFilter("dithering.hlsl", FilterLoader.TargetType.Tex2D);
        }

        private void TestFilter(string name)
        {
            TestFilter(name, FilterLoader.TargetType.Tex2D);
            TestFilter(name, FilterLoader.TargetType.Tex3D);
        }

        private void TestFilter(string name, FilterLoader.TargetType target)
        {
            var loader = new FilterLoader("filter/" + name, target);

            var test = new FilterModel(loader, 1);
        }
    }
}
