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

        public static void ReadTexture<T>(int textureId, int level, PixelFormat format, PixelType type, ref T[] buffer, int x, int y, int width, int height) where T : struct
        {
            var fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
            GL.FramebufferTexture(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, textureId, level);

            // read data
            GL.ReadPixels(0, 0, width, height, format, type, buffer);

            GL.DeleteFramebuffer(fbo);
            Utility.GLCheck();
        }
    }
}
