using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace ViewerTests
{
    [TestClass]
    public class CubeSkippingShaderTest
    {
        [TestMethod]
        public void Compile()
        {
            var s = new CubeSkippingShader();
        }

        [TestMethod]
        public void Checkers3D()
        {
            TestShader("checkers3d.dds");
        }

        [TestMethod]
        public void LongZAxis()
        {
            TestShader("long_z_axis.ktx");
        }

        [TestMethod]
        public void Island()
        {
            TestShader("islands.ktx");
        }

        private void TestShader(string filename)
        {
            var img = IO.LoadImageTexture(TestData.Directory + filename);
            var helpTex = new Texture3D(img.NumMipmaps, img.Size, Format.R8_UInt, true, false);
            var tmpTex = new Texture3D(img.NumMipmaps, img.Size, Format.R8_UInt, true, false);

            var s = new CubeSkippingShader();
            s.Run(img, helpTex, tmpTex, LayerMipmapSlice.Mip0, new UploadBuffer(256));
            var shaderColors = helpTex.GetPixelColors(LayerMipmapSlice.Mip0);
            var shaderValues = new int[img.Size.Product];

            for (int i = 0; i < shaderValues.Length; i++)
            {
                shaderValues[i] = (int)(shaderColors[i].Red + 0.5);
            }

            var expectedValues = CalcValues(img);

            for (int i = 0; i < shaderValues.Length; i++)
            {
                Assert.AreEqual(expectedValues[i], shaderValues[i]);
            }
        }

        private int[] CalcValues(ITexture tex)
        {
            var origColors = tex.GetPixelColors(LayerMipmapSlice.Mip0);
            var size = tex.Size;

            int[] ping = new int[origColors.Length];
            int[] pong = new int[origColors.Length];

            // transfer data into ping
            for (int i = 0; i < origColors.Length; ++i)
            {
                ping[i] = origColors[i].Alpha > 0 ? 0 : 255;
            }

            for (int i = 0; i < 255; ++i)
            {
                foreach (var coord in size)
                {
                    var idx = GetIndex(coord, size);
                    if (ping[idx] == 0)
                    {
                        pong[idx] = 0;
                        continue;
                    }

                    int min = 255;
                    min = Math.Min(min, GetPixelValue(coord - new Size3(1, 0, 0), size, ping));
                    min = Math.Min(min, GetPixelValue(coord + new Size3(1, 0, 0), size, ping));
                    min = Math.Min(min, GetPixelValue(coord - new Size3(0, 1, 0), size, ping));
                    min = Math.Min(min, GetPixelValue(coord + new Size3(0, 1, 0), size, ping));
                    min = Math.Min(min, GetPixelValue(coord - new Size3(0, 0, 1), size, ping));
                    min = Math.Min(min, GetPixelValue(coord + new Size3(0, 0, 1), size, ping));

                    pong[idx] = Math.Min(min + 1, 255);
                }

                int[] temp = ping;
                ping = pong;
                pong = temp;
            }

            return ping;
        }

        private int GetIndex(Size3 index, Size3 size)
        {
            return (index.Z * size.Width * size.Height) + (index.Y * size.Width) + index.X;
        }

        private int GetPixelValue(Size3 coord, Size3 texSize, int[] pong)
        {
            if ((coord >= new Size3(0)).AllTrue() && (coord < texSize).AllTrue())
            {
                return pong[GetIndex(coord, texSize)];
            }

            return 255;
        }
    }
}
