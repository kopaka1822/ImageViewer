using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.ImageLoader;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
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
        private Models models;

        [TestInitialize]
        public void Init()
        {
            models = new Models(1);
            shader = models.SharedModel.Convert;
        }

        [TestMethod]
        public void ConvertFromSrgb()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "small_a.png"));

            var newTex = shader.Convert(tex, Format.R32G32B32A32_Float, models.Scaling);

            TestData.CompareWithSmall(newTex.GetPixelColors(LayerMipmapSlice.Mip0), Color.Channel.Rgba);
        }

        [TestMethod]
        public void ConvertToSrgb()
        {
            // convert from RGBA32F to RGBA8_SRGB
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "small.pfm"));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, models.Scaling);

            TestData.CompareWithSmall(newTex.GetPixelColors(LayerMipmapSlice.Mip0), Color.Channel.Rgb);
        }

        [TestMethod]
        public void ExtractMipmap()
        {
 
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            Assert.AreEqual(3, tex.NumMipmaps);
            TestData.TestCheckersLevel0(tex.GetPixelColors(LayerMipmapSlice.Mip0));
            TestData.TestCheckersLevel1(tex.GetPixelColors(LayerMipmapSlice.Mip1));
            TestData.TestCheckersLevel2(tex.GetPixelColors(LayerMipmapSlice.Mip2));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, models.Scaling, 1);
            Assert.AreEqual(1, newTex.NumMipmaps);
            Assert.AreEqual(2, newTex.Size.Width);
            Assert.AreEqual(2, newTex.Size.Height);

            TestData.TestCheckersLevel1(newTex.GetPixelColors(LayerMipmapSlice.Mip0));
        }

        [TestMethod]
        public void Cropping()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            TestData.TestCheckersLevel0(tex.GetPixelColors(LayerMipmapSlice.Mip0));
            TestData.TestCheckersLevel1(tex.GetPixelColors(LayerMipmapSlice.Mip1));
            TestData.TestCheckersLevel2(tex.GetPixelColors(LayerMipmapSlice.Mip2));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, new LayerMipmapRange(-1, 0), 1.0f, true, new Size3(1, 1, 0), new Size3(2, 2), Size3.Zero, models.Scaling);
            Assert.AreEqual(1, newTex.NumMipmaps);
            Assert.AreEqual(2, newTex.Size.Width);
            Assert.AreEqual(2, newTex.Size.Height);

            // should be the same as first mipmap level
            TestData.TestCheckersLevel1(newTex.GetPixelColors(LayerMipmapSlice.Mip0));
        }

        [TestMethod]
        public void Alignment()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "unaligned.png"));
            Assert.AreEqual(3, tex.Size.Width % 4);
            Assert.AreEqual(1, tex.Size.Height % 4);
            
            // convert with 4 texel alignment
            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb, LayerMipmapSlice.Mip0, 1.0f, false, Size3.Zero, Size3.Zero, new Size3(4, 4, 0), models.Scaling);
            Assert.AreEqual(0, newTex.Size.Width % 4);
            Assert.AreEqual(0, newTex.Size.Height % 4);
        }

        [TestMethod]
        public void Multiplier()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));

            // multiply with 0.5f
            var newTex = shader.Convert(tex, Format.B8G8R8A8_UNorm_SRgb, models.Scaling, 1, 0, 0.5f);
            var colors = newTex.GetPixelColors(LayerMipmapSlice.Mip0);

            Assert.AreEqual(2, newTex.Size.Width);
            Assert.AreEqual(2, newTex.Size.Height);

            // black remains the same
            Assert.IsTrue(Colors.Black.Equals(colors[0], Color.Channel.Rgba));
            Assert.IsTrue(Colors.Black.Equals(colors[3], Color.Channel.Rgba));

            // white changes to gray
            var gray = new Color(0.5f, 0.5f, 0.5f, 1.0f).ToSrgb();
            Assert.IsTrue(gray.Equals(colors[1], Color.Channel.Rgba));
            Assert.IsTrue(gray.Equals(colors[2], Color.Channel.Rgba));
        }

        [TestMethod]
        public void Scaling()
        {
            var tex = IO.LoadImageTexture(TestData.Directory + "checkers.dds");

            // upscale mipmap 1 (should be equivalent to mipmap 0 then)
            var newTex = shader.Convert(tex, tex.Format, LayerMipmapSlice.Mip1, 1.0f, false, Size3.Zero, Size3.Zero, Size3.Zero,
                models.Scaling, null, 2);

            Assert.AreEqual(newTex.Size.Width, 4);
            Assert.AreEqual(newTex.Size.Height, 4);
            Assert.AreEqual(newTex.NumMipmaps, 1);

            TestData.TestCheckersLevel0(newTex.GetPixelColors(LayerMipmapSlice.Mip0));
        }

        [TestMethod]
        public void ScalingAndCrop()
        {
            var tex = IO.LoadImageTexture(TestData.Directory + "checkers.dds");

            // upscale mipmap 1 (should be equivalent to mipmap 0 then)
            var newTex = shader.Convert(tex, tex.Format, new LayerMipmapRange(0,  -1), 1.0f, true, new Size3(1, 1, 0), new Size3(2, 2, 1), Size3.Zero,
                models.Scaling, null, 4);

            Assert.AreEqual(newTex.Size.Width, 8);
            Assert.AreEqual(newTex.Size.Height, 8);
            Assert.AreEqual(newTex.NumMipmaps, 4);

            TestData.TestCheckersLevel0(newTex.GetPixelColors(LayerMipmapSlice.Mip1));
            TestData.TestCheckersLevel1(newTex.GetPixelColors(LayerMipmapSlice.Mip2));
            TestData.TestCheckersLevel2(newTex.GetPixelColors(LayerMipmapSlice.Mip3));
        }

        // not convert format but other conversions
        [TestMethod]
        public void ConvertTo3DCompile()
        {
            var s = new ConvertTo3DShader(new QuadShader());
        }
    }
}
