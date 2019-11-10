using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
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

            Assert.AreEqual(2, res.Size.Width);
            Assert.AreEqual(2, res.Size.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel1(colors);
        }

        [TestMethod]
        public void MinifyCheckersMissingMipmaps()
        {
            var model = new ThumbnailModel(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            var lvl0Checkers = checkers.CloneWithoutMipmapsT();

            var res = model.CreateThumbnail(2, lvl0Checkers, Format.R8G8B8A8_UNorm_SRgb, 0);

            Assert.AreEqual(2, res.Size.Width);
            Assert.AreEqual(2, res.Size.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel1(colors);
        }

        [TestMethod]
        public void MagnifyCheckers()
        {
            var model = new ThumbnailModel(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            var lvl1Checkers = checkers.CloneWithoutMipmapsT(1);

            var res = model.CreateThumbnail(4, lvl1Checkers, Format.R8G8B8A8_UNorm_SRgb, 0);

            Assert.AreEqual(4, res.Size.Width);
            Assert.AreEqual(4, res.Size.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel0(colors);
        }

        [TestMethod]
        public void Image3D()
        {
            var model = new ThumbnailModel(new QuadShader());
            var checkers = new Texture3D(IO.LoadImage(TestData.Directory + "checkers3d.dds"));
            var res = model.CreateThumbnail(4, checkers, Format.R8G8B8A8_UNorm_SRgb, 0);

            Assert.AreEqual(4, res.Size.Width);
            Assert.AreEqual(4, res.Size.Height);

            var colors = res.GetPixelColors(0, 0);
            TestData.TestCheckersLevel0(colors);
        }
    }
}
