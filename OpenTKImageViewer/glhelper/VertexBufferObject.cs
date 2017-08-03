using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class VertexBufferObject
    {
        public int Id { get; private set; }
        public VertexBufferObject(float[] data)
        {
            Id = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, Id);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * data.Length, data, BufferUsageHint.StaticDraw);
        }
    }
}
