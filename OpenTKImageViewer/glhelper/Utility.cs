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
            

            string str = Marshal.PtrToStringAnsi(message, length);
            //if (str ==
            //    "API_ID_RECOMPILE_FRAGMENT_SHADER performance warning has been generated. Fragment shader recompiled due to state change."
            //)
            //    return;

            if (type != DebugType.DebugTypeOther && type != DebugType.DebugTypePerformance)
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

            GL.Arb.DebugMessageControl(All.DontCare, All.DontCare, All.DontCare, 0, (int[])null, false);
            GL.Arb.DebugMessageControl(All.DontCare, All.DontCare, All.DebugSeverityMedium, 0, (int[])null, true);
            GL.Arb.DebugMessageControl(All.DontCare, All.DontCare, All.DebugSeverityHigh, 0, (int[])null, true);
            GL.Arb.DebugMessageCallback(OpenGlDebug, IntPtr.Zero);
        }

        public static void ReadTexture<T>(int textureId, int level, PixelFormat format, PixelType type, ref T[] buffer) where T : struct
        {
            GL.GetTextureImage(textureId, level, format, type, buffer.Length * Marshal.SizeOf(buffer[0]), buffer);
            Utility.GLCheck();
        }
    }
}
