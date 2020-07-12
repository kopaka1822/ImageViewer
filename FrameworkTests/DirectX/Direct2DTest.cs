using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.DirectX
{
    [TestClass]
    public class Direct2DTest
    {
        [TestMethod]
        public void DrawCheckers()
        {
            var img = new TextureArray2D(LayerMipmapCount.One, new Size3(4, 4), Format.R32G32B32A32_Float, false);

            // create checkers texture
            var d2d = new Direct2D(img);
            using (var c = d2d.Begin())
            {
                c.Clear(Colors.White);
                c.FillRectangle(Float2.Zero, new Float2(2.0f), Colors.Black);
                c.FillRectangle(new Float2(2.0f), new Float2(4.0f), Colors.Black);
            }

            var colors = img.GetPixelColors(LayerMipmapSlice.Mip0);

            TestData.TestCheckersLevel0(colors);
        }

        [TestMethod]
        public void DrawText()
        {
            var img = new TextureArray2D(LayerMipmapCount.One, new Size3(16, 16), Format.R8G8B8A8_UNorm_SRgb, false);
            var d2d = new Direct2D(img);
            using (var c = d2d.Begin())
            {
                c.Clear(Colors.White);
                c.Text(Float2.Zero, new Float2(16.0f, 16.0f), 16.0f, Colors.Black, "a!");
            }

            // compare with reference
            var orig = IO.LoadImageTexture(TestData.Directory + "a.png");
            var origColors = orig.GetPixelColors(LayerMipmapSlice.Mip0);
            var newColors = img.GetPixelColors(LayerMipmapSlice.Mip0);
                
            TestData.CompareColors(origColors, newColors);
        }
    }
}
