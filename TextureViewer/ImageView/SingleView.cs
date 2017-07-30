using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Shaders;
using TextureViewer.glhelper;
using TextureViewer.ImageView.Shader;

namespace TextureViewer.ImageView
{
    class SingleView : IImageView
    {
        private class ImageData
        {
            private readonly ImageLoaderWrapper.Image image;
            // textures[0] = Layers with mipmap 0
            private List<TextureArray2D> textures;
            public ImageData(ImageLoaderWrapper.Image image)
            {
                this.image = image;
            }

            public void Init(OpenGL gl)
            {
                if (textures == null)
                {
                    textures = new List<TextureArray2D>(image.GetNumMipmaps());
                    for (int mipmap = 0; mipmap < image.GetNumMipmaps(); ++mipmap)
                    {
                        textures.Add(new TextureArray2D(gl, image, mipmap));
                    }
                }
            }

            public void Bind(uint slot, Context context)
            {
                textures[(int) context.ActiveMipmap].Bind(slot, context.LinearInterpolation?OpenGL.GL_LINEAR:OpenGL.GL_NEAREST);
            }
        }

        private OpenGL gl;
        private SingleViewShader shader;
        private MainWindow parent;
        private Vector curTranslation = new Vector(0.0, 0.0);
        private double curScale = 1.0;
        private List<ImageData> textures = new List<ImageData>();

        public void Init(OpenGL gl, MainWindow parent)
        {
            this.gl = gl;
            this.parent = parent;
            shader = new SingleViewShader(parent.Context);
            shader.Init(gl);

            for(int i = 0; i < parent.Context.GetNumImages(); ++i)
                textures.Add(new ImageData(parent.Context.GetImages()[i]));
        }

        public void Draw()
        {
            // init all images which are not initialized yet
            foreach (var imageData in textures)
            {
                imageData.Init(gl);
            }

            shader.Bind(gl, ApplyScale() * ApplyAspectRatio() * ApplyTranslation());
            
            for (uint texture = 0; texture < textures.Count; ++texture)
            {
                textures[(int) texture].Bind(texture, parent.Context);
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
            Matrix mat = new Matrix(4,4);
            mat[0, 0] = (double) parent.Context.GetWidth((int) parent.Context.ActiveMipmap) /
                        (double) parent.GetClientWidth();
            mat[1, 1] = (double) parent.Context.GetHeight((int) parent.Context.ActiveMipmap) /
                        (double) parent.GetClientHeight();
            mat[2, 2] = 1.0;
            mat[3, 3] = 1.0;
            return mat;
        }

        private Matrix ApplyTranslation()
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = 1.0;
            mat[1, 1] = 1.0;
            mat[2, 2] = 1.0;
            mat[3, 3] = 1.0;
            mat[0, 3] = curTranslation.X;
            mat[1, 3] = curTranslation.Y;
            return mat;
        }

        private Matrix ApplyScale()
        {
            Matrix mat = new Matrix(4, 4);
            mat[0, 0] = curScale;
            mat[1, 1] = curScale;
            mat[2, 2] = 1.0;
            mat[3, 3] = 1.0;
            return mat;
        }

        public void OnDrag(Vector diff)
        {
            // translate into local space
            curTranslation += WindowToClient(diff);
        }

        public void OnScroll(double diff, Point mouse)
        {
            curScale = Math.Min(Math.Max(curScale * (1.0 + (diff * 0.001)), 0.01), 100.0);
        }

        public void SetImageFilter(uint glImageFilter)
        {
            // TODO
        }

        private Vector WindowToClient(Vector vec)
        {
            return new Vector(
                vec.X * 2.0 / parent.Context.GetWidth((int) parent.Context.ActiveMipmap) / curScale,
                -vec.Y * 2.0 / parent.Context.GetHeight((int)parent.Context.ActiveMipmap) / curScale
                );
        }
    }
}
