using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model
{
    [TestClass]
    public class ImagePipelineTest
    {
        [TestMethod]
        public void NoImages()
        {
            var model = new Models(1);
            // nothing should happen and nothing should crash
            model.Apply();
        }

        [TestMethod]
        public void SingleImage()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "small.png");
            model.Apply();
            // look at the generated image
            Assert.IsNotNull(model.Pipelines[0].Image);
            var colors = model.Pipelines[0].Image.GetPixelColors(0, 0);
            TestData.CompareWithSmall(colors, Color.Channel.Rgb);
        }

        [TestMethod]
        public void CombinedImages()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "checkers_left.png");
            model.AddImageFromFile(TestData.Directory + "checkers_right.png");
            model.Pipelines[0].Color.Formula = "I0 + I1";
            model.Apply();

            Assert.IsNotNull(model.Pipelines[0].Image);
            var colors = model.Pipelines[0].Image.GetPixelColors(0, 0);
            TestData.TestCheckersLevel0(colors);
        }

        [TestMethod]
        public void GammaFilter()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "small.pfm");
            var filter = model.CreateFilter("filter/gamma.hlsl");
            // set factor to 4
            foreach (var param in filter.Parameters)
            {
                if (param.GetBase().Name == "Factor")
                {
                    param.GetFloatModel().Value = 4.0f;
                }
            }
            model.Filter.AddFilter(filter);

            model.Apply();
            var colors = model.Pipelines[0].Image.GetPixelColors(0, 0);

            // compare with reference image
            var refColors = TestData.GetColors("smallx4.pfm");

            TestData.CompareColors(refColors, colors);
        }

        [TestMethod]
        public void MedianFilter()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "sphere_salt.png");
            model.Filter.AddFilter(model.CreateFilter("filter/median.hlsl"));
            model.Apply();
            var colors = model.Pipelines[0].Image.GetPixelColors(0, 0);

            // compare with refence image
            var refColors = TestData.GetColors("sphere_median.png");

            TestData.CompareColors(refColors, colors);
        }

        [TestMethod]
        public void BlurFilter()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "sphere.png");
            model.Filter.AddFilter(model.CreateFilter("filter/blur.hlsl"));
            model.Apply();
            var colors = model.Pipelines[0].Image.GetPixelColors(0, 0);

            // compare with refence image
            var refColors = TestData.GetColors("sphere_blur.png");

            TestData.CompareColors(refColors, colors);
        }

        [TestMethod]
        public void OptimizeTrivialFormula()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "small.png");
            model.AddImageFromFile(TestData.Directory + "small.pfm");
            model.Apply();

            Assert.IsTrue(ReferenceEquals(model.Pipelines[0].Image, model.Images.Images[0].Image));

            model.Pipelines[0].Color.Formula = "I1";
            model.Pipelines[0].Alpha.Formula = "I1";
            model.Apply();
            Assert.IsTrue(ReferenceEquals(model.Pipelines[0].Image, model.Images.Images[1].Image));
        }
    }
}
