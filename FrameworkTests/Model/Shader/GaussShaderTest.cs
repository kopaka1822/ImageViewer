using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class GaussShaderTest
    {
        [TestMethod]
        public void Compile()
        {
            var s = new GaussShader(5, 2.25f, "float");
            s.CompileShaders();

            s = new GaussShader(5, 2.25f, "float4");
            s.CompileShaders();
        }

        [TestMethod]
        public void ResultSmall()
        {
            var s = new GaussShader(1, 1.0f, "float4");

            var img = IO.LoadImageTexture(TestData.Directory + "small.pfm");
            Assert.AreEqual(img.Size.Width, 3);
            Assert.AreEqual(img.Size.Height, 3);
            var cache = new TextureCache(img);
            var dst = cache.GetTexture();

            s.Run(img, dst, LayerMipmapSlice.Mip0, new UploadBuffer(256), cache);

            var origColors = img.GetPixelColors(LayerMipmapSlice.Mip0);
            var expectedColors = new Color[origColors.Length];
 
            // do gauss blur with radius 1
            var dim = img.Size;
            for(int x = 0; x < dim.Width; ++x)
            for (int y = 0; y < dim.Height; ++y)
            {
                // calc for pixel xy
                var c = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                var weightSum = 0.0f;
                for(int ox = x - 1; ox <= x + 1; ++ox)
                for (int oy = y - 1; oy <= y + 1; ++oy)
                {
                    if (ox >= 0 && ox < dim.Width && oy >= 0 && oy < dim.Height)
                    {
                        // is inside
                        var w = Kernel(x - ox) * Kernel(y - oy);
                        var sample = origColors[oy * dim.Width + ox];
                        c.Red += sample.Red * w;
                        c.Green += sample.Green * w;
                        c.Blue += sample.Blue * w;
                        c.Alpha += sample.Alpha * w;
                        weightSum += w;
                    }
                }

                c.Red /= weightSum;
                c.Green /= weightSum;
                c.Blue /= weightSum;
                c.Alpha /= weightSum;
                expectedColors[y * dim.Width + x] = c;
            }

            var gaussColors = dst.GetPixelColors(LayerMipmapSlice.Mip0);

            TestData.CompareColors(expectedColors, gaussColors, Color.Channel.Rgba);
        }

        private float Kernel(int offset)
        {
            return (float)Math.Exp(-0.5 * offset * offset);
        }

        // not directly gauss shader but uses gauss weights as well:

        [TestMethod]
        public void VarianceCompile()
        {
            var s = new VarianceShader(5, 2.25f);
            s.CompileShaders();
        }

        [TestMethod]
        public void CorrelationCoefficientCompile()
        {
            var s = new CorrelationCoefficientShader(5, 2.25f);
            s.CompileShaders();
        }

        // test shader with a gauss kernel => should have same result as seperatable shader
        class NonSepGaussShader : NonSepKernelShader
        {
            public NonSepGaussShader() : base(1, new []{"in_values"}, @"
float weightSum = 0.0;
float4 pixelSum = 0.0;
", @"
int radius2 = dot(id - coord, id - coord); // current squared radius
float w = exp(-0.5 * radius2); // gauss weight
weightSum += w;
pixelSum += w * in_values[texel(coord)];
", @"
out_image[texel(id, layer)] = pixelSum / weightSum;
", "float4")
            {
            }

            public void Run(ITexture values, ITexture dst, LayerMipmapSlice lm,
                UploadBuffer upload)
            {
                Run(new[] { values }, dst, lm, upload);
            }
        }

        [TestMethod]
        public void NonSepGaussSmall()
        {
            // sep was tested in previous unit test (ResultSmall)
            var sep = new GaussShader(1, 1.0f, "float4");
            var nonSep = new NonSepGaussShader();

            var img = IO.LoadImageTexture(TestData.Directory + "small.pfm");

            var cache = new TextureCache(img);
            var dstRef = cache.GetTexture();
            var dstNonSep = cache.GetTexture();

            sep.Run(img, dstRef, LayerMipmapSlice.Mip0, new UploadBuffer(256), cache);
            nonSep.Run(img, dstNonSep, LayerMipmapSlice.Mip0, new UploadBuffer(256));

            var expected = dstRef.GetPixelColors(LayerMipmapSlice.Mip0);
            var actual = dstNonSep.GetPixelColors(LayerMipmapSlice.Mip0);

            TestData.CompareColors(expected, actual, Color.Channel.Rgba);
        }
    }
}
