using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.ImageLoader
{
    [TestClass]
    public class DllTest
    {
        public static string Directory = "../../FrameworkTests/TestData/";
        private static readonly Color[] smallData = new Color[]
        {
            new Color(1.0f, 0.0f, 0.0f, 0.5f), new Color(0.0f, 1.0f, 0.0f, 0.5f), new Color(0.0f, 0.0f, 1.0f, 0.5f),
            new Color(0.0f, 0.0f, 0.0f, 0.5f), new Color(0.2122308f, 0.2122308f, 0.2122308f, 0.5f),   new Color(0.0f, 0.0f, 0.0f, 0.5f),
            new Color(1.0f, 1.0f, 1.0f, 0.5f),new Color(1.0f, 1.0f, 1.0f, 0.5f),new Color(1.0f, 1.0f, 1.0f, 0.5f)
        };

        public static void CompareWithSmall(Image image, Color.Channel channels)
        {
            var tex = new TextureArray2D(image);
            var colors = tex.GetPixelColors(0, 0);

            CompareWithSmall(colors, channels);
        }

        public static void CompareWithSmall(Color[] colors, Color.Channel channels)
        {
            Assert.AreEqual(smallData.Length, colors.Length);
            for (int i = 0; i < colors.Length; ++i)
            {
                Assert.IsTrue(colors[i].Equals(smallData[i], channels, 0.02f)); // high tolerance because of jpg compression
            }
        }

        private void VerifySmallLdr(Image image, Color.Channel channels)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R8G8B8A8_UNorm_SRgb, image.Format.DxgiFormat);

            CompareWithSmall(image, channels);
        }

        public void VerifySmallHdr(Image image, Color.Channel channels)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R32G32B32A32_Float, image.Format.DxgiFormat);

            CompareWithSmall(image, channels);
        }

        [TestMethod]
        public void StbiLdr()
        {
            VerifySmallLdr(IO.LoadImage(Directory + "small.png"), Color.Channel.Rgb);

            VerifySmallLdr(IO.LoadImage(Directory + "small_a.png"), Color.Channel.Rgba);

            VerifySmallLdr(IO.LoadImage(Directory + "small.bmp"), Color.Channel.Rgb);

            VerifySmallLdr(IO.LoadImage(Directory + "small.jpg"), Color.Channel.Rgb);
        }

        [TestMethod]
        public void StbiHdr()
        {
            VerifySmallHdr(IO.LoadImage(Directory + "small.hdr"), Color.Channel.Rgb);
        }

        [TestMethod]
        public void PfmColor()
        {
            VerifySmallHdr(IO.LoadImage(Directory + "small.pfm"), Color.Channel.Rgb);
        }

        [TestMethod]
        public void PfmGrayscale()
        {
            VerifySmallHdr(IO.LoadImage(Directory + "small_g.pfm"), Color.Channel.R);
        }

        [TestMethod]
        public void DDSSimple()
        {
            VerifySmallHdr(IO.LoadImage(Directory + "small.dds"), Color.Channel.Rgba);
        }

        [TestMethod]
        public void KTXSimple()
        {
            VerifySmallHdr(IO.LoadImage(Directory + "small.ktx"), Color.Channel.Rgba);
        }
    }
}
