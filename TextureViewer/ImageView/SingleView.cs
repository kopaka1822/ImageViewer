using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpGL;
using SharpGL.SceneGraph.Shaders;
using TextureViewer.glhelper;

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
        private ShaderProgram program;
        private MainWindow parent;
        private Vector curTranslation = new Vector(0.0, 0.0);
        private double curScale = 1.0;
        private List<ImageData> textures = new List<ImageData>();

        public void Init(OpenGL gl, MainWindow parent)
        {
            this.gl = gl;
            this.parent = parent;

            LoadShader();

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

            program.Push(gl, null);
            Utility.GlCheck(gl);

            // TODO select correct layer in shader
            for (uint texture = 0; texture < textures.Count; ++texture)
            {
                textures[(int) texture].Bind(texture, parent.Context);
                Utility.GlCheck(gl);
            }

            ApplyAspectRatio();
            ApplyScale();
            ApplyTranslation();
            Utility.GlCheck(gl);

            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);

            gl.Vertex(1.0f, -1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, 1.0f, 0.0f);

            gl.End();
            Utility.GlCheck(gl);

            program.Pop(gl, null);
            Utility.GlCheck(gl);
        }

        private void LoadShader()
        {
            VertexShader vertexShader = new VertexShader();
            vertexShader.CreateInContext(gl);
            vertexShader.SetSource(
                "varying vec2 texcoord; void main() { texcoord = (gl_Vertex.xy + vec2(1.0, -1.0)) * vec2(0.5, -0.5); gl_Position = gl_ProjectionMatrix * gl_ModelViewMatrix * vec4(gl_Vertex.xy, 0.0, 1.0); }");

            FragmentShader fragmentShader = new FragmentShader();
            fragmentShader.CreateInContext(gl);
            fragmentShader.SetSource(
                "uniform sampler2DArray tex; varying vec2 texcoord; void main() { vec2 texel = texcoord /*+ vec2(1.0) / vec2(textureSize(tex, 0))*/; gl_FragColor = texture(tex, vec3(texel, 0.0)); }");

            vertexShader.Compile();
            fragmentShader.Compile();
            if (vertexShader.CompileStatus.HasValue && vertexShader.CompileStatus.Value == false)
                throw new Exception("vertex shader: " + vertexShader.InfoLog);
            if (fragmentShader.CompileStatus.HasValue && fragmentShader.CompileStatus.Value == false)
                throw new Exception("fragment shader:" + fragmentShader.InfoLog);

            program = new ShaderProgram();
            program.CreateInContext(gl);

            program.AttachShader(vertexShader);
            program.AttachShader(fragmentShader);
            program.Link();
        }

        private void ApplyAspectRatio()
        {
            gl.Scale((float)parent.Context.GetWidth((int) parent.Context.ActiveMipmap) / (float)parent.GetClientWidth(),
                (float)parent.Context.GetHeight((int)parent.Context.ActiveMipmap) / (float)parent.GetClientHeight(), 1.0f);
        }

        private void ApplyTranslation()
        {
            gl.Translate(curTranslation.X, curTranslation.Y, 0.0);
        }

        private void ApplyScale()
        {
            gl.Scale(curScale, curScale, 1.0);
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
