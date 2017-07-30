using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpGL;
using SharpGL.SceneGraph;
using TextureViewer.glhelper;
using TextureViewer.ImageView.Shader;

namespace TextureViewer.ImageView
{
    public class CubeMapView : IImageView
    {
        private class ImageData
        {
            private readonly ImageLoaderWrapper.Image image;
            // textures[0] = Layers with mipmap 0
            private List<TextureCubeMap> textures;
            public ImageData(ImageLoaderWrapper.Image image)
            {
                this.image = image;
            }

            public void Init(OpenGL gl)
            {
                if (textures == null)
                {
                    textures = new List<TextureCubeMap>(image.GetNumMipmaps());
                    for (int mipmap = 0; mipmap < image.GetNumMipmaps(); ++mipmap)
                    {
                        textures.Add(new TextureCubeMap(gl, image, mipmap));
                    }
                }
            }

            public void Bind(uint slot, Context context)
            {
                textures[(int)context.ActiveMipmap].Bind(slot, context.LinearInterpolation ? OpenGL.GL_LINEAR : OpenGL.GL_NEAREST);
            }
        }

        private OpenGL gl;
        private CubeMapShader shader;
        private List<ImageData> textures = new List<ImageData>();
        private MainWindow parent;
        private float yawn = 0.0f;
        private float pitch = 0.0f;
        private float roll = 0.0f;

        public void Init(OpenGL gl, MainWindow parent)
        {
            this.gl = gl;
            this.parent = parent;
            this.shader = new CubeMapShader(parent.Context);
            shader.Init(gl);

            for (int i = 0; i < parent.Context.GetNumImages(); ++i)
                textures.Add(new ImageData(parent.Context.GetImages()[i]));
        }

        public void Draw()
        {
            // init all images which are not initialized yet
            foreach (var imageData in textures)
            {
                imageData.Init(gl);
            }

            shader.Bind(gl, ApplyAspectRatio() * ApplyRotation() * ApplyOrientation());

            for (uint texture = 0; texture < textures.Count; ++texture)
            {
                textures[(int)texture].Bind(texture, parent.Context);
                Utility.GlCheck(gl);
            }

            Utility.GlCheck(gl);

            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);

            gl.Vertex(1.0f, -1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, 1.0f, 0.0f);

            gl.End();
            Utility.GlCheck(gl);

            shader.Unbind(gl);
        }

        private Matrix ApplyAspectRatio()
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = (double)parent.Context.GetWidth((int)parent.Context.ActiveMipmap) /
                        (double)parent.GetClientWidth();
            mat[1, 1] = (double)parent.Context.GetHeight((int)parent.Context.ActiveMipmap) /
                        (double)parent.GetClientHeight();
            mat[2, 2] = 1.0;
            mat[3, 3] = 1.0;
            return mat;
        }

        private Matrix ApplyRotation()
        {
            return ZRotation(yawn) * YRotation(pitch) * XRotation(roll);
        }

        private Matrix XRotation(float a)
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = 1.0;
            mat[1, 1] = Math.Cos(a);
            mat[1, 2] = -Math.Sin(a);
            mat[2, 1] = Math.Sin(a);
            mat[2, 2] = Math.Cos(a);
            mat[3, 3] = 1.0;
            return mat;
        }

        private Matrix YRotation(float a)
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = Math.Cos(a);
            mat[0, 2] = Math.Sin(a);
            mat[1, 1] = 1.0;
            mat[2, 2] = Math.Cos(a);
            mat[2, 0] = -Math.Sin(a);
            mat[3, 3] = 1.0;
            return mat;
        }

        private Matrix ZRotation(float a)
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = Math.Cos(a);
            mat[0, 1] = -Math.Sin(a);
            mat[1, 0] = Math.Sin(a);
            mat[1, 1] = Math.Cos(a);
            mat[2, 2] = 1.0;
            mat[3, 3] = 1.0;
            return mat;
        }

        private Matrix ApplyOrientation()
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = 1.0;
            mat[1, 1] = -1.0;
            mat[2, 2] = 1.0;
            mat[3, 3] = 1.0;
            return mat;
        }

        public void OnDrag(Vector diff)
        {
            pitch += (float)diff.X * 0.01f;
            roll += (float) diff.Y * -0.01f;
        }

        public void OnScroll(double diff, Point mouse)
        {
            
        }
    }
}
