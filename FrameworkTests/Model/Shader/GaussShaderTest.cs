using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Shader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Shader
{
    [TestClass]
    public class GaussShaderTest
    {
        [TestMethod]
        public void Compile()
        {
            var s = new GaussShader(11, 1.5f, new QuadShader());
            s.CompileShaders();
        }
    }
}
