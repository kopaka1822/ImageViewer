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

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class PixelValueShaderTest
    {
        [TestMethod]
        public void SmallImage()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "small.pfm"));
            var refColors = tex.GetPixelColors(0, 0);

            // recreate colors by picking them with the shader
            var shader = new PixelValueShader();
            var colors = new Color[refColors.Length];
            for(int y = 0; y < tex.Height; ++y)
                for (int x = 0; x < tex.Width; ++x)
                {
                    colors[y * tex.Width + x] = shader.Run(tex, x, y, 0, 0, 0);
                }

            TestData.CompareColors(refColors, colors, Color.Channel.Rgb);
        }

        [TestMethod]
        public void Radius()
        {
            var tex = new TextureArray2D(IO.LoadImage(TestData.Directory + "checkers.dds"));
            var shader = new PixelValueShader();

            var color = shader.Run(tex, 1, 1, 0, 0, 1);
            // should be 5 times black field + 4 times white field
            var expected = new Color(4.0f / 9.0f);

            Assert.IsTrue(expected.Equals(color, Color.Channel.Rgb));

            color = shader.Run(tex, 1, 2, 0, 0, 1);
            // should be 5 times white + 4 times black
            expected = new Color(5.0f / 9.0f);

            Assert.IsTrue(expected.Equals(color, Color.Channel.Rgb));
        }
    }
}
