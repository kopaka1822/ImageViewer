using System;
using SharpGL;

namespace TextureViewer.glhelper
{
    public static class Utility
    {
        public static void GlCheck(OpenGL gl)
        {
            var glerr = gl.GetError();
            if (glerr != OpenGL.GL_NO_ERROR)
                throw new Exception(gl.GetErrorDescription(glerr));
        }
    }
   
}