using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Controller;
using TextureViewer.Controller.TextureViews.Shader;
using TextureViewer.glhelper;

namespace TextureViewer.Models
{
    /// <summary>
    /// opengl related data used for texture views
    /// </summary>
    public class OpenGlModel
    {
        public VertexArray Vao { get; }
        public CheckersShader CheckersShader { get; }


        public OpenGlModel(OpenGlController controller)
        {
            Debug.Assert(controller.IsEnabled);
            Vao = new VertexArray();
            CheckersShader = new CheckersShader();
        }

        public void Dispose()
        {
            CheckersShader.Dispose();
            Vao.Dispose();
        }
    }
}
