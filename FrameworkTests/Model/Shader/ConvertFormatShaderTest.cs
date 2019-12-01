using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.ImageLoader;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class ConvertFormatShaderTest
    {
        private ConvertFormatShader shader;

        [TestInitialize]
        public void Init()
        {
            shader = new ConvertFormatShader(new QuadShader());
        }

        [TestCleanup]
        public void Cleanup()
        {
            shader?.Dispose();
        }

        [TestMethod]
        public void ConvertFromSrgb()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "small_a.png"));

            var newTex = shader.Convert(tex, Format.R32G32B32A32_Float);

            TestData.CompareWithSmall(newTex.GetPixelColors(0, 0), Color.Channel.Rgba);
        }

        [TestMethod]
        public void ConvertToSrgb()
        {
            // convert from RGBA32F to RGBA8_SRGB
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "small.pfm"));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb);

            TestData.CompareWithSmall(newTex.GetPixelColors(0, 0), Color.Channel.Rgb);
        }

        [TestMethod]
        public void ExtractMipmap()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            Assert.AreEqual(3, tex.NumMipmaps);
            TestData.TestCheckersLevel0(tex.GetPixelColors(0, 0));
            TestData.TestCheckersLevel1(tex.GetPixelColors(0, 1));
            TestData.TestCheckersLevel2(tex.GetPixelColors(0, 2));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, 1);
            Assert.AreEqual(1, newTex.NumMipmaps);
            Assert.AreEqual(2, newTex.GetWidth(0));
            Assert.AreEqual(2, newTex.GetHeight(0));

            TestData.TestCheckersLevel1(newTex.GetPixelColors(0, 0));
        }

        [TestMethod]
        public void Cropping()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            TestData.TestCheckersLevel0(tex.GetPixelColors(0, 0));
            TestData.TestCheckersLevel1(tex.GetPixelColors(0, 1));
            TestData.TestCheckersLevel2(tex.GetPixelColors(0, 2));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, 0, -1, 1.0f, true, 1, 1, 2, 2, 0, 0);
            Assert.AreEqual(1, newTex.NumMipmaps);
            Assert.AreEqual(2, newTex.GetWidth(0));
            Assert.AreEqual(2, newTex.GetHeight(0));

            // should be the same as first mipmap level
            TestData.TestCheckersLevel1(newTex.GetPixelColors(0, 0));
        }

        [TestMethod]
        public void Alignment()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "unaligned.png"));
            Assert.AreEqual(3, tex.Width % 4);
            Assert.AreEqual(1, tex.Height % 4);
            
            // convert with 4 texel alignment
            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, 0, 0, 1.0f, false, 0, 0, 0, 0, 4, 4);
            Assert.AreEqual(0, newTex.Width % 4);
            Assert.AreEqual(0, newTex.Height % 4);
        }

        [TestMethod]
        public void Multiplier()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));

            // multiply with 0.5f
            var newTex = shader.Convert(tex, Format.B8G8R8A8_UNorm_SRgb, 1, 0, 0.5f);
            var colors = newTex.GetPixelColors(0, 0);

            Assert.AreEqual(2, newTex.Width);
            Assert.AreEqual(2, newTex.Height);

            // black remains the same
            Assert.IsTrue(Color.Black.Equals(colors[0], Color.Channel.Rgba));
            Assert.IsTrue(Color.Black.Equals(colors[3], Color.Channel.Rgba));

            // white changes to gray
            var gray = new Color(0.5f, 0.5f, 0.5f, 1.0f).ToSrgb();
            Assert.IsTrue(gray.Equals(colors[1], Color.Channel.Rgba));
            Assert.IsTrue(gray.Equals(colors[2], Color.Channel.Rgba));
        }
    }
}
