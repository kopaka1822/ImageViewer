using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Scaling;
using ImageFramework.Model.Scaling.Down;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model.Scaling
{
    [TestClass]
    public class DownscalingTest
    {
        [TestMethod]
        public void FastScaling()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers_wide.png");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var src = models.Pipelines[0].Image;
            Assert.IsNotNull(src);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float ,src.Format);
            var mipped = src.CloneWithMipmaps(src.Size.MaxMipLevels);
            // mip levels:
            // 0: 8 x 4
            // 1: 4 x 2
            // 2: 2 x 1
            // 3: 1 x 1

            models.Scaling.WriteMipmaps(mipped);

            // test mipmaps
            var mip1 = mipped.GetPixelColors(LayerMipmapSlice.Mip1);
            var refMip1 = IO.LoadImageTexture(TestData.Directory + "checkers_wide_mip1.png").GetPixelColors(LayerMipmapSlice.Mip0);

            Console.Out.WriteLine("Testing fast results");

            TestData.CompareColors(refMip1, mip1);

            var mip2 = mipped.GetPixelColors(LayerMipmapSlice.Mip2);
            Assert.AreEqual(2, mip2.Length);
            Assert.IsTrue(new Color(0.5f).Equals(mip2[0], Color.Channel.Rgb));

            Console.Out.WriteLine("Testing copy results");

            var mip3 = mipped.GetPixelColors(LayerMipmapSlice.Mip3);
            Assert.AreEqual(1, mip3.Length);
            Assert.IsTrue(new Color(0.5f).Equals(mip3[0], Color.Channel.Rgb));
        }

        [TestMethod]
        public void FastSlowScaling()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers3x7.png");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var src = models.Pipelines[0].Image;
            Assert.IsNotNull(src);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float, src.Format);
            var mipped = src.CloneWithMipmaps(src.Size.MaxMipLevels);
            // mip levels:
            // 0: 3 x 7
            // 1: 1 x 3 fast + slow
            // 2: 1 x 1 fast + fast

            models.Scaling.WriteMipmaps(mipped);

            // test mipmaps
            var mip1 = mipped.GetPixelColors(LayerMipmapSlice.Mip1);
            var refMip1 = IO.LoadImageTexture(TestData.Directory + "checkers3x7_mip1.png").GetPixelColors(LayerMipmapSlice.Mip0);
            TestData.CompareColors(refMip1, mip1);

            var mip2 = mipped.GetPixelColors(LayerMipmapSlice.Mip2);
            Assert.AreEqual(1, mip2.Length);
            Assert.IsTrue(new Color(0.476f).Equals(mip2[0], Color.Channel.Rgb));
        }

        [TestMethod]
        public void OverwriteMipmapsLdr()
        {
            var models = new Models(1);
            // load ldr file (png)
            models.AddImageFromFile(TestData.Directory + "checkers3x7.png");
            models.Images.GenerateMipmaps(models.Scaling);
            models.Apply();

            var mipped = models.Pipelines[0].Image;
            Assert.AreEqual(Format.R8G8B8A8_UNorm_SRgb, mipped.Format);

            // test mipmaps
            var mip1 = mipped.GetPixelColors(LayerMipmapSlice.Mip1);
            var refMip1 = IO.LoadImageTexture(TestData.Directory + "checkers3x7_mip1.png").GetPixelColors(LayerMipmapSlice.Mip0);
            TestData.CompareColors(refMip1, mip1);

            var mip2 = mipped.GetPixelColors(LayerMipmapSlice.Mip2);
            Assert.AreEqual(1, mip2.Length);
            Assert.IsTrue(new Color(0.476f).Equals(mip2[0], Color.Channel.Rgb));
        }

        [TestMethod]
        public void Scaling3D()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers3d.dds");
            models.Pipelines[0].Color.Formula = "I0 + 0"; // trick pipeline to create a rgba32 target for us
            models.Apply();

            var original = models.Images.Images[0].Image;
            var dst = models.Pipelines[0].Image;
            Assert.IsNotNull(dst);
            Assert.AreEqual(SharpDX.DXGI.Format.R32G32B32A32_Float, dst.Format);
            Assert.IsTrue(dst != models.Images.Images[0].Image);

            // overwrite mipmaps
            models.Scaling.WriteMipmaps(dst);

            TestData.CompareColors(original.GetPixelColors(LayerMipmapSlice.Mip1), dst.GetPixelColors(LayerMipmapSlice.Mip1));
            TestData.CompareColors(original.GetPixelColors(LayerMipmapSlice.Mip2), dst.GetPixelColors(LayerMipmapSlice.Mip2));
        }

        [TestMethod]
        public void BoxCompile()
        {
            var s = new BoxScalingShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void TriangleCompile()
        {
            var s = new TriangleScalingShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void LanzosCompile()
        {
            var s = new LanzosScalingShader();
            s.CompileShaders();
        }

        [TestMethod]
        public void DetailPreservingCompile()
        {
            var s = new DetailPreservingDownscalingShader(null, true);
            s.CompileShaders();
        }

        [TestMethod]
        public void CompileDetailPreservingCore()
        {
            var s = new DetailPreservingShaderCore(true);
            s.CompileShaders();
            s = new DetailPreservingShaderCore(false);
            s.CompileShaders();
        }

        [TestMethod]
        public void FastGaussTest()
        {
            // filter kernel:
            // 1 2 1
            // 2 4 2
            // 1 2 1

            var s = new FastGaussShader();
            var img = IO.LoadImageTexture(TestData.Directory + "small.pfm");
            var dst = new TextureArray2D(LayerMipmapCount.One, img.Size, Format.R32G32B32A32_Float, true);

            s.Run(img, dst, 0, false, new UploadBuffer(256));

            var src = img.GetPixelColors(LayerMipmapSlice.Mip0);
            var res = dst.GetPixelColors(LayerMipmapSlice.Mip0);
            Assert.AreEqual(3 * 3, res.Length);

            // expected values calculated by hand
            float midR = (src[0].Red + src[1].Red * 2.0f + src[2].Red
                           + src[3].Red * 2.0f + src[4].Red * 4.0f + src[5].Red * 2.0f
                           + src[6].Red + src[7].Red * 2.0f + src[8].Red) / 16.0f;
            float midG = (src[0].Green + src[1].Green * 2.0f + src[2].Green
                          + src[3].Green * 2.0f + src[4].Green * 4.0f + src[5].Green * 2.0f
                          + src[6].Green + src[7].Green * 2.0f + src[8].Green) / 16.0f;
            var midColor = new Color(midR, midG, 1.0f);

            float firstR = (src[0].Red * 4.0f + src[1].Red * 2.0f
                            + src[3].Red * 2.0f + src[4].Red) / 9.0f;
            var firstColor = new Color(firstR);

            Assert.IsTrue(res[4].Equals(midColor, Color.Channel.R | Color.Channel.G));
            Assert.IsTrue(res[0].Equals(firstColor, Color.Channel.R));
        }

        /// <summary>
        /// tests if the box filter does not lose energy when building mipmaps
        /// </summary>
        [TestMethod]
        public void BoxFilterEnergyConserve()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "sphere.png");
            models.Pipelines[0].Color.Formula = "I0 * RGB(1, 1, 0)";
            models.Pipelines[0].RecomputeMipmaps = true;
            models.Images.GenerateMipmaps(models.Scaling);
            models.Scaling.Minify = ScalingModel.MinifyFilters.Box;
            models.Apply();

            var img = models.Pipelines[0].Image;
            Assert.IsNotNull(img);
            Assert.IsTrue(img.NumMipmaps > 1);
            var refStats = models.Stats.GetStatisticsFor(img, LayerMipmapSlice.Mip0);
            for (int i = 1; i < img.NumMipmaps; ++i)
            {
                var s = models.Stats.GetStatisticsFor(img, new LayerMipmapSlice(0, i));

                Assert.AreEqual(refStats.Average.Avg, s.Average.Avg, 0.01f);
            }
        }

        [TestMethod]
        public void TriangleFastFilterEnergyConserve()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "checkers.dds");
            models.Pipelines[0].Color.Formula = "I0 * RGB(1, 1, 0)";
            models.Images.DeleteMipmaps();
            models.Images.GenerateMipmaps(models.Scaling);
            models.Scaling.Minify = ScalingModel.MinifyFilters.Triangle;
            models.Pipelines[0].RecomputeMipmaps = true;
            models.Apply();

            TestEnergyConserve(models);
        }

        [TestMethod]
        public void TriangleSlowFilterEnergyConserve()
        {
            var models = new Models(1);
            // this filter is actually not 100% energy conservant (i.e. if going from 3x3 to 1x1)
            models.AddImageFromFile(TestData.Directory + "checkers3x7.png");
            models.Pipelines[0].Color.Formula = "I0 * RGB(1, 1, 0)";
            models.Pipelines[0].RecomputeMipmaps = true;
            models.Scaling.Minify = ScalingModel.MinifyFilters.Triangle;
            models.Images.GenerateMipmaps(models.Scaling);
            models.Apply();

            TestEnergyConserve(models);
        }

        private void TestEnergyConserve(Models models)
        {
            var img = models.Pipelines[0].Image;
            Assert.IsNotNull(img);
            Assert.IsTrue(img.NumMipmaps > 1);
            var refStats = models.Stats.GetStatisticsFor(img, LayerMipmapSlice.Mip0);
            for (int i = 1; i < img.NumMipmaps; ++i)
            {
                var s = models.Stats.GetStatisticsFor(img, new LayerMipmapSlice(0, i));

                Assert.AreEqual(refStats.Average.Avg, s.Average.Avg, 0.01f);
            }
        }
    }
}
