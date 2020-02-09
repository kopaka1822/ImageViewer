using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class TransformShaderTest
    {
        [TestMethod]
        public void Compile()
        {
            var s = new TransformShader(TransformShader.TransformLuma, "float4", "float");
            s.CompileShaders();
        }

        [TestMethod]
        public void RunSmall()
        {
            var s = new TransformShader("return float4(value.rgb * 2.0, value.a)", "float4", "float4");
            var img = IO.LoadImageTexture(TestData.Directory + "small.pfm");

            var dst = new TextureArray2D(img.NumLayers, img.NumMipmaps, img.Size, Format.R32G32B32A32_Float, true);
            s.Run(img, dst, 0, 0, new UploadBuffer(256));

            var expected = img.GetPixelColors(0, 0);
            for (var index = 0; index < expected.Length; index++)
            {
                expected[index].Red *= 2.0f;
                expected[index].Green *= 2.0f;
                expected[index].Blue *= 2.0f;
            }

            var actual  = dst.GetPixelColors(0, 0);
            TestData.CompareColors(expected, actual, Color.Channel.Rgba);
        }
    }
}
