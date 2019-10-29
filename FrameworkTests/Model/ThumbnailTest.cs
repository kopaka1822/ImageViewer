using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Shader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model
{
    [TestClass]
    public class ThumbnailTest
    {
        [TestMethod]
        public void MinifyCheckers()
        {
            var model = new ThumbnailModel(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));

            var res = model.CreateThumbnail(2, checkers, Format.R8G8B8A8_UNorm_SRgb, 0);

            Assert.AreEqual(2, res.Width);
            Assert.AreEqual(2, res.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel1(colors);
        }

        [TestMethod]
        public void MinifyCheckersMissingMipmaps()
        {
            var model = new ThumbnailModel(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            var lvl0Checkers = checkers.CloneWithoutMipmaps();

            var res = model.CreateThumbnail(2, lvl0Checkers, Format.R8G8B8A8_UNorm_SRgb, 0);

            Assert.AreEqual(2, res.Width);
            Assert.AreEqual(2, res.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel1(colors);
        }

        [TestMethod]
        public void MagnifyCheckers()
        {
            var model = new ThumbnailModel(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            var lvl1Checkers = checkers.CloneWithoutMipmaps(1);

            var res = model.CreateThumbnail(4, lvl1Checkers, Format.R8G8B8A8_UNorm_SRgb, 0);

            Assert.AreEqual(4, res.Width);
            Assert.AreEqual(4, res.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel0(colors);
        }
    }
}
