using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;

namespace OpenTKImageViewer.UI
{
    class PixelValueShader
    {
        private readonly Program shaderProgram;
        private int bufferId = 0;

        public PixelValueShader()
        {
            shaderProgram = new Program(new List<Shader>{new Shader(ShaderType.ComputeShader, GetComputeSource()).Compile()}, true);
            InitBuffer();
        }

        private void InitBuffer()
        {
            bufferId = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferId);
            GL.BufferStorage(BufferTarget.ShaderStorageBuffer,  new IntPtr(sizeof(float) * 4), 
                IntPtr.Zero, BufferStorageFlags.MapReadBit | BufferStorageFlags.ClientStorageBit);
        }

        public int GetTextureLocation()
        {
            return 0;
        }

        /// <summary>
        /// Retrieves the pixel color from the bound texture
        /// The Texture2D object must be bound to GetTextureLocation() before
        /// calling this function.
        /// </summary>
        /// <param name="x">x coordinate</param>
        /// <param name="y">y coordinate</param>
        /// <param name="radius">summation radius (0 = only this pixel)</param>
        /// <returns></returns>
        public Vector4 GetPixelColor(int x, int y, int radius)
        {
            // set variables
            shaderProgram.Bind();
            SetPixelRadius(radius);
            SetPixelCoord(x,y);

            // buffer binding and dispatch
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, bufferId);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, bufferId);
            GL.DispatchCompute(1,1,1);
            Program.Unbind();

            // retrieve buffer data
            float[] data = new float[4];
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, new IntPtr(0),
                new IntPtr(sizeof(float) * 4), data);
            
            glhelper.Utility.GLCheck();
            return new Vector4(data[0], data[1], data[2], data[3]);
        }

        private static void SetPixelRadius(int radius)
        {
            GL.Uniform1(1, radius);
        }

        private static void SetPixelCoord(int x, int y)
        {
            GL.Uniform2(0, x, y);
        }

        private static string GetComputeSource()
        {
            return "#version 430 core\n" +
                   "layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;\n" +
                   "layout(binding = 0) uniform sampler2D src;\n" +
                   "layout(location = 0) uniform ivec2 pixelCoord;\n" +
                   "layout(location = 1) uniform int pixelRadius;\n" +
                   "\n" +
                   "layout(std430, binding = 1) buffer pixelDstBuffer { vec4 pixelDst; };" +
                   "void main(){\n" +
                   "vec4 sum = vec4(0.0);\n" +
                   "ivec2 size = textureSize(src, 0);\n" +
                   "for(int x = pixelCoord.x - pixelRadius; x <= pixelCoord.x + pixelRadius; ++x)\n" +
                   "for(int y = pixelCoord.y - pixelRadius; y <= pixelCoord.y + pixelRadius; ++y)\n" +
                   "    sum += texelFetch(src, ivec2(   clamp(x,0,size.x-1),\n" +
                                                        // y coords are reverted
                   "                                    clamp(size.y - y - 1,0,size.y-1)), 0);\n" +
                   
                   "int width = 1 + 2 * pixelRadius;\n" +
                   "pixelDst = sum / ivec4(width * width);\n" +
                   "}";
        }

        public void Dispose()
        {
            shaderProgram?.Dispose();
        }
    }
}
