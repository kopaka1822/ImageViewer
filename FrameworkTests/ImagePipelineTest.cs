using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests
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
            model.AddImageFromFile(DllTest.Directory + "small.png");
            model.Apply();
            // look at the generated image
            Assert.IsNotNull(model.Pipelines[0].Image);
            var colors = model.Pipelines[0].Image.GetPixelColors(0, 0);
            DllTest.CompareWithSmall(colors, Color.Channel.Rgb);
        }
    }
}
