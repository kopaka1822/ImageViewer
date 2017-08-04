using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class Mesh
    {
        private VertexArrayObject vao;
        private readonly PrimitiveType type;
        private readonly int indexCount;

        public Mesh(VertexArrayObject vao, PrimitiveType type, int indexCount)
        {
            this.vao = vao;
            this.type = type;
            this.indexCount = indexCount;
        }

        public void Draw()
        {
            vao.Draw(type, indexCount);
        }

        public static Mesh GenerateQuad()
        {
            VertexBufferObject vbo = new VertexBufferObject(new float[]
            /*{
                1.0f, -1.0f,
                -1.0f, -1.0f,
                1.0f, 1.0f,
                -1.0f, 1.0f
            });*/
                {
                    0.5f, -0.5f,
                    -0.5f, -0.5f,
                    0.5f, 0.5f,
                    -0.5f, 0.5f
                });
            VertexArrayObject vao = new VertexArrayObject();
            vao.AddVertexBuffer(vbo, 0, 2);

            return new Mesh(vao, PrimitiveType.TriangleStrip, 4);
        }
    }
}
