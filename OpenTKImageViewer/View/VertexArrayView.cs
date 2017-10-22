using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.View
{
    public abstract class VertexArrayView : IImageView
    {
        private int vertexArrayId = 0;

        public virtual void Update(MainWindow window)
        {
            if(vertexArrayId == 0)
                vertexArrayId = GL.GenVertexArray();
        }

        protected void DrawQuad()
        {
            Debug.Assert(vertexArrayId != 0);
            GL.BindVertexArray(vertexArrayId);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public virtual void Draw(int activeImage)
        {
            
        }

        public virtual void OnDrag(Vector diff, MainWindow window)
        {
            
        }

        public virtual void OnScroll(double diff, Point mouse)
        {
        }

        public virtual void UpdateMouseDisplay(MainWindow window)
        {
            
        }

        public virtual void SetZoom(float dec)
        {
            
        }

        public virtual void Dispose()
        {
            if (vertexArrayId != 0)
            {
                GL.DeleteVertexArray(vertexArrayId);
                vertexArrayId = 0;
            }
        }

        /// <summary>
        /// transforms mouse coordinates from range [-1, 1] to [0, imageSize] and clamps the range if input exceeds [-1, 1]
        /// </summary>
        /// <param name="transMouse"> [-1, 1]</param>
        /// <param name="imageWidth">width of the current image</param>
        /// <param name="imageHeight">height of the current image</param>
        /// <returns>[0, 1]</returns>
        public static Vector4 MouseToTextureCoordinates(Vector4 transMouse, int imageWidth, int imageHeight)
        {
            // trans mouse is betweem [-1,1] in texture coordinates => to [0,1]
            transMouse.X += 1.0f;
            transMouse.X /= 2.0f;

            transMouse.Y += 1.0f;
            transMouse.Y /= 2.0f;

            // clamp value
            transMouse.X = Math.Min(0.9999f, Math.Max(0.0f, transMouse.X));
            transMouse.Y = Math.Min(0.9999f, Math.Max(0.0f, transMouse.Y));

            // scale with mipmap level
            transMouse.X *= (float)imageWidth;
            transMouse.Y *= (float)imageHeight;

            return transMouse;
        }
    }
}
