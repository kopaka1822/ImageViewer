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

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(string file)
        {
            InitializeComponent();
            this.Title = "Texture Viewer - Panel - " + file;
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
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(1.0f, -1.0f, 0.0f);
            gl.Vertex(-1.0f, -1.0f, 0.0f);
            gl.Vertex(1.0f, 1.0f, 0.0f);
            gl.Vertex(-1.0f, 1.0f, 0.0f);

            gl.End();

            //  Flush OpenGL.
            gl.Flush();
        }

        private void OpenGLControl_OnOpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            OpenGL gl = args.OpenGL;
            
        }
    }
}
