using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.View
{
    public abstract class VertexArrayView : IImageView
    {
        private int vertexArrayId = 0;

        public virtual void Update()
        {
            if(vertexArrayId == 0)
                vertexArrayId = GL.GenVertexArray();
        }

        public virtual void Draw()
        {
            Debug.Assert(vertexArrayId != 0);
            GL.BindVertexArray(vertexArrayId);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        public virtual void OnDrag(Vector diff)
        {
            
        }

        public virtual void OnScroll(double diff, Point mouse)
        {
        }
    }
}
