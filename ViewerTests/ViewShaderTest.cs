using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Shader;
using ImageViewer.Controller.TextureViews.Shader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ViewerTests
{
    /// <summary>
    /// tests if the view shaders compile
    /// </summary>
    [TestClass]
    public class ViewShaderTest
    {
        [TestMethod]
        public void CheckersCompile()
        {
            CheckersShader.CompileShaders();
        }

        [TestMethod]
        public void CubeCompile()
        {
            var cube = new CubeViewShader(null);
        }

        [TestMethod]
        public void PolarCompile()
        {
            var polar = new PolarViewShader(null);
        }

        [TestMethod]
        public void SmoothVolumeCompile()
        {
            var ray = new SmoothVolumeShader(null);
        }

        [TestMethod]
        public void CubeVolumeCompile()
        {
            var ray = new CubeVolumeShader(null);
        }

        [TestMethod]
        public void ShearWarpCompile()
        {
            var shear = new ShearWarpShader(null);
        }

        [TestMethod]
        public void SingleView2DCompile()
        {
            var sv = new SingleViewShader(null, ShaderBuilder.Builder2D);
        }

        [TestMethod]
        public void SingleView3DCompile()
        {
            var sv = new SingleViewShader(null, ShaderBuilder.Builder3D);
        }
    }
}
