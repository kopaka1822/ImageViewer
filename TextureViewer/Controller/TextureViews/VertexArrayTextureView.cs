using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace TextureViewer.Controller.TextureViews
{
    public class VertexArrayTextureView : ITextureView
    {
        private int vertexArrayId = 0;

        public void Draw()
        {
            throw new NotImplementedException();
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
        protected void DrawQuad()
        {
            Debug.Assert(vertexArrayId != 0);
            GL.BindVertexArray(vertexArrayId);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}
