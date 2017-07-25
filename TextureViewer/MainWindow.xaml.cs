using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Shaders;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShaderProgram program;
        private ImageLoaderWrapper.Image image;

        public MainWindow(ImageLoaderWrapper.Image file)
        {
            this.image = file;

            InitializeComponent();
            this.Title = "Texture Viewer - Panel - ";
        }
        

        private void OpenGLControl_OnOpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //  Get the OpenGL instance that's been passed to us.
            OpenGL gl = args.OpenGL;
          

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            //  Clear the color and depth buffers.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            //  Reset the modelview matrix.
            gl.LoadIdentity();

            //  Start drawing triangles.
            program.Push(gl, null);
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            
            gl.Vertex(1.0f, -1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, 1.0f, 0.0f);

            gl.End();
            program.Pop(gl, null);

            //  Flush OpenGL.
            gl.Flush();
        }

        private void OpenGLControl_OnOpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            
            VertexShader vertexShader = new VertexShader();
            vertexShader.CreateInContext(gl);
            vertexShader.SetSource("void main() { gl_Position =  vec4(gl_Vertex.xy, 0.0, 1.0); }");

            FragmentShader fragmentShader = new FragmentShader();
            fragmentShader.CreateInContext(gl);
            fragmentShader.SetSource("void main() { gl_FragColor = vec4(1.0,1.0,0.0,0.0); }");

            vertexShader.Compile();
            fragmentShader.Compile();

            program = new ShaderProgram();
            program.CreateInContext(gl);

            program.AttachShader(vertexShader);
            program.AttachShader(fragmentShader);
            program.Link();


        }
    }
}
