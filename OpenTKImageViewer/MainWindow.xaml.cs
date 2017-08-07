using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using OpenTKImageViewer.ImageContext;
using OpenTKImageViewer.View;
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

        #region VARIABLES
        private readonly App parent;
        private GLControl glControl;

        private Program _program;
        private int _vertexArray;
        private double _time;

        private string error = "";
        private int iteration = 0;
        
        public ulong ZIndex { get; set; }
        private ImageContext.ImageContext Context { get; set; }
        private Dictionary<ImageViewType, IImageView> imageViews = new Dictionary<ImageViewType, IImageView>();
        private ImageViewType currentImageView;
        public ImageViewType CurrentView
        {
            get { return currentImageView; }
            set
            {
                if (imageViews.ContainsKey(value))
                    currentImageView = value;
            }
        }

        #endregion

        #region INITIALIZATION

        public MainWindow(App parent, ImageContext.ImageContext context)
        {
            this.parent = parent;
            this.Context = context;
            this.ZIndex = 0;

            InitializeComponent();
            CreateImageViews();
        }

        private void CreateImageViews()
        {
            if (Context.GetNumImages() > 0)
            {
                // TODO add other views
            }

            if (imageViews.Count == 0)
            {
                imageViews.Add(ImageViewType.Empty, new EmptyView());
                CurrentView = ImageViewType.Empty;
            }
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
                error = exception.Message + ": " + exception.StackTrace;
            }
        }

        private void InitGraphics()
        { 
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            _program = CreateProgram();
            GL.GenVertexArrays(1, out _vertexArray);
        }

#endregion

        private Program CreateProgram()
        {
            try
            {
                var shaders = new List<Shader>();
                shaders.Add(new Shader(ShaderType.VertexShader,
                    "#version 450 core\n" +
                    "layout (location = 1) in vec4 position;\n" +
                    "void main(void){\n" +
                    "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                    "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                    "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                    "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                    "if(gl_VertexID == 3u) vertex = vec4(-1.0, 0.5, 0.0, 1.0);\n" +
                    "gl_Position = vertex;\n" +
                    "}\n").Compile());
                
                shaders.Add(new Shader(ShaderType.FragmentShader,
                    "#version 450 core\n" +
                    "out vec4 color;\n" +
                    "void main(void){\n" +
                    "color = vec4(1.0);\n" +
                    "}\n").Compile());
                
                Program program = new Program(shaders, true);
                
                return program;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
        

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (error.Length > 0 && iteration++ > 0)
            {
                MessageBox.Show(error);
                error = "";
            }

            try
            {
                GL.Viewport(0, 0, (int)WinFormsHost.ActualWidth, (int)WinFormsHost.ActualHeight);
                GL.ClearColor(0.9333f, 0.9333f, 0.9333f, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                
                imageViews[CurrentView]?.Update();

                //_program.Bind();
                //GL.BindVertexArray(_vertexArray);

                //GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                imageViews[CurrentView]?.Update();
                
                glControl.SwapBuffers();
            }
            catch (Exception exception)
            {
                if (error.Length == 0)
                    error = exception.Message + ": " + exception.StackTrace;
            }

            glControl.Invalidate();
        }

        private void WinFormsHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
