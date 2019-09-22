using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.ImageLoader
{
    [TestClass]
    public class DllTest
    {
        public string Directory = "../../FrameworkTests/TestData/";

        private void VerifySmall3Ldr(Image image)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R8G8B8A8_UNorm, image.Format.Format);
            Assert.AreEqual(true, image.Format.IsSrgb);
            Assert.AreEqual(false , image.Format.HasAlpha);
            
        }

        private void VerifySmall4Ldr(Image image)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R8G8B8A8_UNorm, image.Format.Format);
            Assert.AreEqual(true, image.Format.IsSrgb);
            Assert.AreEqual(true, image.Format.HasAlpha);

        }

        public void VerifySmall4Hdr(Image image)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R32G32B32A32_Float, image.Format.Format);
            Assert.AreEqual(false, image.Format.IsSrgb);
            Assert.AreEqual(true, image.Format.HasAlpha);
        }

        public void VerifySmall3Hdr(Image image)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R32G32B32_Float, image.Format.Format);
            Assert.AreEqual(false, image.Format.IsSrgb);
            Assert.AreEqual(false, image.Format.HasAlpha);
        }

        public void VerifySmall1Hdr(Image image)
        {
            Assert.AreEqual(1, image.NumMipmaps);
            Assert.AreEqual(1, image.NumLayers);
            Assert.AreEqual(3, image.GetWidth(0));
            Assert.AreEqual(3, image.GetHeight(0));
            Assert.AreEqual(Format.R32_Float, image.Format.Format);
            Assert.AreEqual(false, image.Format.IsSrgb);
            Assert.AreEqual(false, image.Format.HasAlpha);
        }

        [TestMethod]
        public void StbiLdr()
        {
            VerifySmall3Ldr(IO.LoadImage(Directory + "small.png"));

            VerifySmall4Ldr(IO.LoadImage(Directory + "small_a.png"));

            VerifySmall3Ldr(IO.LoadImage(Directory + "small.bmp"));

            VerifySmall3Ldr(IO.LoadImage(Directory + "small.jpg"));
        }

        [TestMethod]
        public void StbiHdr()
        {
            VerifySmall3Hdr(IO.LoadImage(Directory + "small.hdr"));
        }

        [TestMethod]
        public void PfmColor()
        {
            VerifySmall3Hdr(IO.LoadImage(Directory + "small.pfm"));
        }

        [TestMethod]
        public void PfmGrayscale()
        {
            VerifySmall1Hdr(IO.LoadImage(Directory + "small_g.pfm"));
        }

        [TestMethod]
        public void DDSSimple()
        {
            VerifySmall4Hdr(IO.LoadImage(Directory + "small.dds"));
        }

        [TestMethod]
        public void KTXSimple()
        {
            VerifySmall4Hdr(IO.LoadImage(Directory + "small.ktx"));
        }
    }
}
