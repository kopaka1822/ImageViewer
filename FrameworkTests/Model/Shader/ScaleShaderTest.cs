using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Export;
using ImageFramework.Model.Shader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class ScaleShaderTest
    {
        /*[TestMethod]
        public void MitchelUpscale()
        {
            var shader = new MitchellNetravaliScaleShader();
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere.png"));

            var res = shader.Run(checkers, 62, 31);
            var export = new ExportModel();
            export.Mipmap = 0;
            var des = new ExportDescription( TestData.Directory + "upscale", "png", export) {FileFormat = GliFormat.RGB8_SRGB};
            export.Export(res, des);
        }*/
    }
}
