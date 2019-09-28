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
        public void ConvertFromSrgb()
        {
            var tex = new TextureArray2D(IO.LoadImage(DllTest.Directory + "small_a.png"));

            var newTex = shader.Convert(tex, Format.R32G32B32A32_Float);

            DllTest.CompareWithSmall(newTex.GetPixelColors(0, 0), Color.Channel.Rgba);
        }

        [TestMethod]
        public void ConvertToSrgb()
        {
            // convert from RGBA32F to RGBA8_SRGB
            var tex = new TextureArray2D(IO.LoadImage(DllTest.Directory + "small.pfm"));

            var newTex = shader.Convert(tex, Format.R8G8B8A8_UNorm_SRgb);

            DllTest.CompareWithSmall(newTex.GetPixelColors(0, 0), Color.Channel.Rgb);
        }
    }
}
