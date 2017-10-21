using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.UI
{
    class PixelValueShader
    {
        private readonly Program shaderProgram;

        public PixelValueShader()
        {
            shaderProgram = new Program(new List<Shader>{new Shader(ShaderType.ComputeShader, GetComputeSource()).Compile()}, true);
        }

        public Vector4 GetPixelColor()
        {
            return new Vector4();
        }

        private static string GetComputeSource()
        {
            return "#version 430 core\n" +
                   "";
        }
    }
}
