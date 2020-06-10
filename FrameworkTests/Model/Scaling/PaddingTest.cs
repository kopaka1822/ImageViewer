using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Scaling
{
    [TestClass]
    public class PaddingTest
    {
        [TestMethod]
        public void Compile()
        {
            var models = new Models(1);
            var s = models.SharedModel.Padding;
        }

        [TestMethod]
        public void LeftPadBlack()
        {
            var models = new Models(1);
            var t = IO.LoadImageTexture(TestData.Directory + "small.pfm");

            var res = models.SharedModel.Padding.Run(t, new Size3(1, 0, 0), Size3.Zero, PaddingShader.FillMode.Black,
                models.Scaling, models.SharedModel);

            Assert.AreEqual(4, res.Size.Width);
            Assert.AreEqual(3, res.Size.Height);

            var origColors = t.GetPixelColors(LayerMipmapSlice.Mip0);
            var newColors = res.GetPixelColors(LayerMipmapSlice.Mip0);

            // test border
            Assert.IsTrue(Colors.Black.Equals(newColors[0]));
            Assert.IsTrue(Colors.Black.Equals(newColors[4]));
            Assert.IsTrue(Colors.Black.Equals(newColors[8]));

            // first row
            Assert.IsTrue(origColors[0].Equals(newColors[1]));
            Assert.IsTrue(origColors[1].Equals(newColors[2]));
            Assert.IsTrue(origColors[2].Equals(newColors[3]));

            // second row
            Assert.IsTrue(origColors[3].Equals(newColors[5]));
            Assert.IsTrue(origColors[4].Equals(newColors[6]));
            Assert.IsTrue(origColors[5].Equals(newColors[7]));

            // third row
            Assert.IsTrue(origColors[6].Equals(newColors[9]));
            Assert.IsTrue(origColors[7].Equals(newColors[10]));
            Assert.IsTrue(origColors[8].Equals(newColors[11]));
        }
    }
}
