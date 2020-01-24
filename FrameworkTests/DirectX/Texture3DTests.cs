using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.DirectX
{
    [TestClass]
    public class Texture3DTests
    {
        [TestMethod]
        public void TestMipmapLoad()
        {
            var tex = new Texture3D(IO.LoadImage(TestData.Directory + "checkers3d.dds"));
            Assert.AreEqual(4, tex.Size.Width);
            Assert.AreEqual(4, tex.Size.Height);
            Assert.AreEqual(4, tex.Size.Depth);
            Assert.AreEqual(3, tex.NumMipmaps);

            TestData.TestCheckers3DLevel0(tex.GetPixelColors(0));
            TestData.TestCheckers3DLevel1(tex.GetPixelColors(1));
            TestData.TestCheckersLevel2(tex.GetPixelColors(2));
        }
    }
}
