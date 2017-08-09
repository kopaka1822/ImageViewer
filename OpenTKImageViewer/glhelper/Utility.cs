using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public static class Utility
    {
        public static void GLCheck()
        {
            var glerr = GL.GetError();
            if (glerr != ErrorCode.NoError)
              throw new Exception(glerr.ToString());
        }
    }
}
