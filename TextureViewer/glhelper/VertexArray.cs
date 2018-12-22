using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller;

namespace TextureViewer.glhelper
{
    public class VertexArray
    {
        private int vertexArrayId = 0;

        public VertexArray()
        {
            vertexArrayId = GL.GenVertexArray();
        }

        /// <summary>
        /// disposing vertex array object
        /// </summary>
        public void Dispose()
        {
            if (vertexArrayId != 0)
            {
                GL.DeleteVertexArray(vertexArrayId);
                vertexArrayId = 0;
            }
        }

        /// <summary>
        /// Call to DrawArrays with count = 4 (no data attached)
        /// </summary>
        public void DrawQuad()
        {
            Debug.Assert(vertexArrayId != 0);
            GL.BindVertexArray(vertexArrayId);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}
