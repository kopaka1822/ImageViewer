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
        [TestMethod]
        public void MitchelUpscale()
        {
            var shader = new MitchellNetravaliScaleShader(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere.png"));

            var res = shader.Run(checkers, 62, 31);

            var reference = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere_up.png"));

            // compare with gimp reference
            TestData.CompareColors(reference.GetPixelColors(0, 0), res.GetPixelColors(0, 0));
        }

        [TestMethod]
        public void MitchelXYScale()
        {
            var shader = new MitchellNetravaliScaleShader(new QuadShader());
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere.png"));

            var res = shader.Run(checkers, 20, 40);

            var reference = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere_scaled.png"));

            // compare with gimp reference
            TestData.CompareColors(reference.GetPixelColors(0, 0), res.GetPixelColors(0, 0));
        }
    }
}
