using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;
using BeginMode = OpenTK.Graphics.OpenGL.BeginMode;
using MatrixMode = OpenTK.Graphics.OpenGL.MatrixMode;
using MessageBox = System.Windows.MessageBox;

namespace OpenTKImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GLControl glControl;

        private Mesh mesh;
        private Program program;

        private string error = "";
        private int iteration = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WinFormsHost_OnInitialized(object sender, EventArgs e)
        {
            try
            {
                var flags = GraphicsContextFlags.Default;
                glControl = new GLControl(new GraphicsMode(32, 24), 4, 2, flags);
                glControl.Paint += GLControl_Paint;
                glControl.Dock = DockStyle.Fill;
                var windowsFormsHost = sender as WindowsFormsHost;
                if (windowsFormsHost != null) windowsFormsHost.Child = glControl;

                glControl.MakeCurrent();
                InitGraphics();
            }
            catch (Exception exception)
            {
                error = exception.Message;
            }
        }

        private void InitGraphics()
        {
            //mesh = Mesh.GenerateQuad();

            // make Shader
            var vertexShader = new Shader(ShaderType.VertexShader);
            vertexShader.Source += "#version 420\n";
            //vertexShader.Source += "layout(location = 0) in vec2 vertex;\n";
            vertexShader.Source += "void main(){\n";
            //vertexShader.Source += "gl_Position = vec4(vertex, 0.0, 1.0);\n";
            vertexShader.Source += "if(gl_VertexID == 0u) gl_Position = vec4(1.0, -1.0, 0.0, 1.0);";
            vertexShader.Source += "if(gl_VertexID == 1u) gl_Position = vec4(-1.0, -1.0, 0.0, 1.0);";
            vertexShader.Source += "if(gl_VertexID == 2u) gl_Position = vec4(1.0, 1.0, 0.0, 1.0);";
            vertexShader.Source += "if(gl_VertexID == 3u) gl_Position = vec4(-1.0, 1.0, 0.0, 1.0);";
            vertexShader.Source += "}";
            vertexShader.Compile();

            var fragmentShader = new Shader(ShaderType.FragmentShader);
            fragmentShader.Source += "#version 420\n";
            fragmentShader.Source += "out vec4 color;\n";
            fragmentShader.Source += "void main(){\n";
            fragmentShader.Source += "color = vec4(1.0);\n";
            fragmentShader.Source += "}";

            var shaders = new List<Shader> {vertexShader, fragmentShader};

            program = new Program(shaders);
        }

        private float r = 0.0f;
        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (error.Length > 0 && iteration++ > 0)
            {
                MessageBox.Show(error);
                error = "";
            }

            try
            {
                r += 0.001f;
                glControl.MakeCurrent();
                GL.ClearColor(r, 0.933f, 0.933f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);

                GL.UseProgram(0);

                program.Bind();
                //mesh.Draw();
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                GL.Flush();
                GL.Finish();
                glControl.SwapBuffers();

            }
            catch (Exception exception)
            {
                if (error.Length == 0)
                    error = exception.Message;
            }

            glControl.Invalidate();
        }

        private void WinFormsHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
