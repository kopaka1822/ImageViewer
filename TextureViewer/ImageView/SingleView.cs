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
        private OpenGL gl;
        private ShaderProgram program;
        private TextureArray2D texture;
        private MainWindow parent;
        private int curMipmap;
        private Vector curTranslation = new Vector(0.0, 0.0);
        private double curScale = 1.0;

        public void Init(OpenGL gl, MainWindow parent)
        {
            this.gl = gl;
            this.parent = parent;
            this.curMipmap = 0;

            LoadShader();
            // TODO load all layer
            texture = new TextureArray2D(gl, parent.Image, 0);
        }

        public void Draw()
        {
            program.Push(gl, null);
            texture.Bind(0);

            ApplyAspectRatio();
            ApplyScale();
            ApplyTranslation();

            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);

            gl.Vertex(1.0f, -1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, 1.0f, 0.0f);

            gl.End();
            program.Pop(gl, null);
        }

        private void LoadShader()
        {
            VertexShader vertexShader = new VertexShader();
            vertexShader.CreateInContext(gl);
            vertexShader.SetSource(
                "varying vec2 texcoord; void main() { texcoord = (gl_Vertex.xy + vec2(1.0)) * vec2(0.5); gl_Position = gl_ProjectionMatrix * gl_ModelViewMatrix * vec4(gl_Vertex.xy, 0.0, 1.0); }");

            FragmentShader fragmentShader = new FragmentShader();
            fragmentShader.CreateInContext(gl);
            fragmentShader.SetSource(
                "uniform sampler2DArray tex; varying vec2 texcoord; void main() {  gl_FragColor = texture(tex, vec3(texcoord, 0.0)); }");

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
            gl.Scale((float)parent.Image.GetWidth(curMipmap) / (float)parent.GetClientWidth(),
                (float)parent.Image.GetHeight(curMipmap) / (float)parent.GetClientHeight(), 1.0f);
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
            curTranslation += WindowToClient(diff) / curScale;
        }

        public void OnScroll(double diff)
        {
            curScale = Math.Min(Math.Max(curScale * (1.0 + (diff * 0.001)), 0.01), 100.0);
        }

        public void SetImageFilter(uint glImageFilter)
        {
            texture.FilterMode = glImageFilter;
        }


        private Vector WindowToClient(Vector vec)
        {
            return new Vector(
                vec.X * 2.0 / parent.Image.GetWidth(curMipmap),
                -vec.Y * 2.0 / parent.Image.GetHeight(curMipmap)
                );
        }
    }
}
