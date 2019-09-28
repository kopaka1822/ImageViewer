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
        public void TestCheckersLevel0(Color[] colors)
        {
            Assert.AreEqual(4 * 4, colors.Length);
            Assert.IsTrue(colors[0].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[1].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[2].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[3].Equals(Color.White, Color.Channel.Rgb));

            Assert.IsTrue(colors[4].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[5].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[6].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[7].Equals(Color.White, Color.Channel.Rgb));

            Assert.IsTrue(colors[8].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[9].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[10].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[11].Equals(Color.Black, Color.Channel.Rgb));

            Assert.IsTrue(colors[12].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[13].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[14].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[15].Equals(Color.Black, Color.Channel.Rgb));
        }

        public void TestCheckersLevel1(Color[] colors)
        {
            Assert.AreEqual(2 * 2, colors.Length);
            Assert.IsTrue(colors[0].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[1].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[2].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[3].Equals(Color.Black, Color.Channel.Rgb));
        }

        public void TestCheckersLevel2(Color[] colors)
        {
            Assert.AreEqual(1, colors.Length);
            Assert.IsTrue(colors[0].Equals(new Color(0.5f, 0.5f, 0.5f), Color.Channel.Rgb));
        }

        [TestMethod]
        public void TestMipmaps()
        {
            // load checkers texture
            var tex = new TextureArray2D(IO.LoadImage(DllTest.Directory + "checkers.dds"));
            Assert.AreEqual(3, tex.NumMipmaps);
            Assert.AreEqual(4, tex.Width);
            Assert.AreEqual(4, tex.Height);
            TestCheckersLevel0(tex.GetPixelColors(0, 0));
            TestCheckersLevel1(tex.GetPixelColors(0, 1));
            TestCheckersLevel2(tex.GetPixelColors(0, 2));

            // remove mipmaps
            tex = tex.CloneWithoutMipmaps();
            Assert.AreEqual(1, tex.NumMipmaps);
            Assert.AreEqual(4, tex.Width);
            Assert.AreEqual(4, tex.Height);
            TestCheckersLevel0(tex.GetPixelColors(0, 0));

            // generate mipmaps again
            tex = tex.GenerateMipmapLevels(3);
            Assert.AreEqual(3, tex.NumMipmaps);
            Assert.AreEqual(4, tex.Width);
            Assert.AreEqual(4, tex.Height);
            TestCheckersLevel0(tex.GetPixelColors(0, 0));
            TestCheckersLevel1(tex.GetPixelColors(0, 1));
            TestCheckersLevel2(tex.GetPixelColors(0, 2));
        }
    }
}
