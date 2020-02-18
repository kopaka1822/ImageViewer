using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class ScaleShaderTest
    {
        [TestMethod]
        public void MitchelUpscale()
        {
            var models = new Models(1);
            var shader = new MitchellNetravaliScaleShader(new QuadShader(), new UploadBuffer(256));
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere.png"));

            var res = shader.Run(checkers, new Size3(62, 31), models.Scaling);

            var reference = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere_up.png"));

            // compare with gimp reference
            TestData.CompareColors(reference.GetPixelColors(LayerMipmapSlice.Mip0), res.GetPixelColors(LayerMipmapSlice.Mip0));
        }

        [TestMethod]
        public void MitchelXYScale()
        {
            var models = new Models(1);
            var shader = new MitchellNetravaliScaleShader(new QuadShader(), new UploadBuffer(256));
            var checkers = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere.png"));

            var res = shader.Run(checkers, new Size3(20, 40), models.Scaling);

            var reference = new TextureArray2D(IO.LoadImage(TestData.Directory + "sphere_scaled.png"));

            // compare with gimp reference
            TestData.CompareColors(reference.GetPixelColors(LayerMipmapSlice.Mip0), res.GetPixelColors(LayerMipmapSlice.Mip0));
        }
    }
}
