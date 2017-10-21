using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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

        public static void OpenGlDebug(DebugSource source, DebugType type, int id, DebugSeverity severity, int length,
            IntPtr message, IntPtr userParam)
        {
            // NVIDIA: "shader will be recompiled due to GL state mismatches"

            if (id == 131218)
                return;
            

            string str = Marshal.PtrToStringAnsi(message, length);
            if (str ==
                "API_ID_RECOMPILE_FRAGMENT_SHADER performance warning has been generated. Fragment shader recompiled due to state change."
            )
                return;

            if (type != DebugType.DebugTypeOther)
            {
                App.ShowErrorDialog(null, $"{source}({severity}): {str}");
            }
            else
            {
#if DEBUG
                App.ShowInfoDialog(null, $"{source}({severity}): {str}");
#endif
            }
        }

        public static void EnableDebugCallback()
        {
            GL.Enable(EnableCap.DebugOutput);

            GL.Arb.DebugMessageControl(All.DontCare, All.DebugTypeError, All.DebugSeverityHigh, 0, new int[0], true);
            GL.Arb.DebugMessageCallback(OpenGlDebug, IntPtr.Zero);
        }

        public static void ReadTexture<T>(int textureId, int level, PixelFormat format, PixelType type, ref T[] buffer, int x, int y, int width, int height) where T : struct
        {
            GL.GetTextureImage(textureId, level, format, type, buffer.Length * Marshal.SizeOf(buffer[0]), buffer);
            Utility.GLCheck();
            /*EnableDebugCallback();

            var id = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelPackBuffer, id);
            GL.BufferData(BufferTarget.PixelPackBuffer, buffer.Length * Marshal.SizeOf(buffer[0]), IntPtr.Zero, BufferUsageHint.StaticRead);

            Utility.GLCheck();
            //GL.Disable(EnableCap.DebugOutput);
            // copying texture into bound pixel pack buffer
            GL.GetTextureImage(id, level, format, type, buffer.Length * Marshal.SizeOf(buffer[0]), IntPtr.Zero);
            Utility.GLCheck();
            
            // upload to cpu
            GL.GetBufferSubData(BufferTarget.PixelPackBuffer, IntPtr.Zero, new IntPtr(Marshal.SizeOf(buffer[0]) * buffer.Length), buffer);
            //Utility.GLCheck();

            GL.Enable(EnableCap.DebugOutput);
            GL.DeleteBuffer(id);
            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
            /*var fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo);
            GL.FramebufferTexture(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, textureId, level);

            // read data
            GL.ReadPixels(0, 0, width, height, format, type, buffer);

            GL.DeleteFramebuffer(fbo);
            Utility.GLCheck();*/
        }
    }
}
