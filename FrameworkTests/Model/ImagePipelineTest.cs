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
    }
}
