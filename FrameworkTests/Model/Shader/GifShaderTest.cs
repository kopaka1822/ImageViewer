using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Shader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model.Shader
{
    /*[TestClass]
    public class GifShaderTest
    {
        [TestMethod]
        public void GifBorder()
        {
            var shader = new GifShader(new QuadShader());
            var left = new TextureArray2D(IO.LoadImage(TestData.Directory + "gray.png"));
            var right = new TextureArray2D(IO.LoadImage(TestData.Directory + "blue.png"));
            var reference = new TextureArray2D(IO.LoadImage(TestData.Directory + "mixed.png"));
            var dst = new TextureArray2D(1, 1, left.Width, left.Height, Format.R8G8B8A8_UNorm_SRgb, false);

            shader.Run(left.GetSrView(0, 0), right.GetSrView(0, 0), dst.GetRtView(0, 0), 
                1, 2, left.Width, left.Height);

            var dstColors = dst.GetPixelColors(LayerMipmapSlice.Mip0);
            var refColors = reference.GetPixelColors(LayerMipmapSlice.Mip0);
            TestData.CompareColors(refColors, dstColors);
        }

        [TestMethod]
        public void GifModel()
        {
            var model = new GifModel(new QuadShader());

            var left = new TextureArray2D(IO.LoadImage(TestData.Directory + "gray.png"));
            var right = new TextureArray2D(IO.LoadImage(TestData.Directory + "blue.png"));

            // just test if it does not crash for now
            model.CreateGif(left, right, new GifModel.Config
            {
                Filename = ExportTest.ExportDir + "animated.gif",
                FramesPerSecond = 10,
                NumSeconds = 5,
                SliderWidth = 1
            });
        }
    }*/
}
