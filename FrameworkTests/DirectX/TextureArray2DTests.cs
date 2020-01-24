using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.ImageLoader;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.DirectX
{
    [TestClass]
    public class TextureArray2DTests
    {
        

        [TestMethod]
        public void TestMipmaps()
        {
            // load checkers texture
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            Assert.AreEqual(3, tex.NumMipmaps);
            Assert.AreEqual(4, tex.Size.Width);
            Assert.AreEqual(4, tex.Size.Height);
            TestData.TestCheckersLevel0(tex.GetPixelColors(0, 0));
            TestData.TestCheckersLevel1(tex.GetPixelColors(0, 1));
            TestData.TestCheckersLevel2(tex.GetPixelColors(0, 2));

            // remove mipmaps
            tex = tex.CloneWithoutMipmapsT();
            Assert.AreEqual(1, tex.NumMipmaps);
            Assert.AreEqual(4, tex.Size.Width);
            Assert.AreEqual(4, tex.Size.Height);
            TestData.TestCheckersLevel0(tex.GetPixelColors(0, 0));
        }
    }
}
