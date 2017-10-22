using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class VertexArrayObject : IGlObject
    {
        private int id;
        private List<VertexBufferObject> vbos = new List<VertexBufferObject>();

        public VertexArrayObject()
        {
            id = GL.GenVertexArray();
        }

        public void AddVertexBuffer(VertexBufferObject buffer, int slot, int attrSize)
        {
            GL.BindVertexArray(id);
            GL.EnableVertexAttribArray(slot);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Id);
            GL.VertexAttribPointer(
                slot,
                attrSize,
                VertexAttribPointerType.Float, 
                false,
                0,
                0);

            vbos.Add(buffer);
        }

        public void Draw(PrimitiveType mode, int count)
        {
            GL.BindVertexArray(id);
            GL.EnableVertexAttribArray(0);
            GL.DrawArrays(mode, 0, count);
        }

        public void Dispose()
        {
            if (id != 0)
            {
                GL.DeleteVertexArray(id);
                id = 0;
            }
            if (vbos != null)
            {
                foreach (var vertexBufferObject in vbos)
                {
                    vertexBufferObject.Dispose();
                }
                vbos = null;
            }
        }
    }
}
