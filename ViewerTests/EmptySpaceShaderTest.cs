using System;
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
    public class EmptySpaceShaderTest
    {
        [TestMethod]
        public void Checkers3DTest()
        {
            TestEmptySpaceGeneration("checkers3d.dds");
        }

        [TestMethod]
        public void LongZAxisTest()
        {
            TestEmptySpaceGeneration("long_z_axis.ktx");
        }

        [TestMethod]
        public void IslandTest()
        {
            TestEmptySpaceGeneration("islands.ktx");
        }



        private void TestEmptySpaceGeneration(String filename)
        {
            var models = new ImageFramework.Model.Models(1);

            var img = IO.LoadImageTexture(TestData.Directory + filename);
            var helpTex = new Texture3D(img.NumMipmaps, img.Size, Format.R8_UInt, true, false);

            var s = new EmptySpaceSkippingShader();
            s.Execute(img, helpTex, LayerMipmapSlice.Mip0, new UploadBuffer(256));


            var shaderResultColors = helpTex.GetPixelColors(LayerMipmapSlice.Mip0);
            //get values from the red channel
            int[] shaderValues = new int[img.Size.Product];
            for (int i = 0; i < shaderValues.Length; i++)
            {
                shaderValues[i] = (int)(shaderResultColors[i].Red + 0.5);
            }


            int[] expectedValues = calcEmptySpace(ref img);



            for (int i = 0; i < shaderValues.Length; i++)
            {
                Assert.AreEqual(expectedValues[i], shaderValues[i]);
            }

        }


        private int[] calcEmptySpace(ref ITexture inputTex)
        {
            var origColors = inputTex.GetPixelColors(LayerMipmapSlice.Mip0);
            var expectedColors = new Color[origColors.Length];
            Size3 size = inputTex.Size;

            int[] ping = new int[origColors.Length];
            int[] pong = new int[origColors.Length];



            for (int i = 0; i < 127; i++)
            {
                foreach (var coord in size)
                {

                    int curIndex = getIndex(coord, size);
                    if (origColors[curIndex].Alpha > 0)
                    {
                        ping[curIndex] = 0;
                    }
                    else
                    {
                        int min = 126;
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int y = -1; y <= 1; y++)
                            {
                                for (int z = -1; z <= 1; z++)
                                {
                                    min = Math.Min(min, getPixelValue(coord + new Size3(x, y, z), size, pong));
                                }
                            }
                        }
                        ping[curIndex] = min + 1;
                    }

                }

                int[] temp = ping;
                ping = pong;
                pong = temp;

            }

            return pong;
        }

        private int getIndex(Size3 index, Size3 size)
        {
            return (index.Z * size.Width * size.Height) + (index.Y * size.Width) + index.X;
        }

        //returns value of the pixel or 127 if out of range
        private int getPixelValue(Size3 coord, Size3 texSize, int[] pong)
        {
            if ((coord >= new Size3(0)).AllTrue() && (coord < texSize).AllTrue())
            {
                return pong[getIndex(coord, texSize)];
            }
            else
            {
                return 127;
            }



        }


    }
}
