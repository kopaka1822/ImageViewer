using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.ImageLoader;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class ShaderModelTest
    {
        private ShaderModel shader;

        [TestInitialize]
        public void Init()
        {
            shader = new ShaderModel();
        }

        [TestMethod]
        public void ConvertSimple()
        {
            // load simple image and convert from RGB to R
            var tex = new TextureArray2D(IO.LoadImage(DllTest.Directory + "small.pfm"));

            var newTex = shader.Convert(tex,
                new ImageFormat {Format = Format.R32G32B32A32_Float, HasAlpha = false, IsSrgb = false});

            DllTest.CompareWithSmall(newTex.GetPixelColors(0, 0), Color.Channel.Rgb);
        }

        [TestMethod]
        public void ConvertSrgb()
        {
            var tex = new TextureArray2D(IO.LoadImage(DllTest.Directory + "small_a.png"));

            var newTex = shader.Convert(tex,
                new ImageFormat { Format = Format.R32G32B32A32_Float, HasAlpha = false, IsSrgb = false });

            DllTest.CompareWithSmall(newTex.GetPixelColors(0, 0), Color.Channel.Rgba);
        }
    }
}
