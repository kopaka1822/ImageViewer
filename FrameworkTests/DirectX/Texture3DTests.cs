using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.DirectX
{
    [TestClass]
    public class Texture3DTests
    {

        [TestMethod]
        public void TestMipmaps()
        {
            var tex = new Texture3D(IO.LoadImage(TestData.Directory + "checkers3d.dds"));
            Assert.AreEqual(4, tex.Width);
            Assert.AreEqual(4, tex.Height);
            Assert.AreEqual(4, tex.Depth);
            Assert.AreEqual(3, tex.NumMipmaps);


        }
    }
}
