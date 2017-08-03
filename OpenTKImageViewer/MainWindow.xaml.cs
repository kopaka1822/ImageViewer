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

namespace OpenTKImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GLControl glControl;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WinFormsHost_OnInitialized(object sender, EventArgs e)
        {
            var flags = GraphicsContextFlags.Default;
            glControl = new GLControl(new GraphicsMode(32, 24), 2, 0, flags);
            glControl.Paint += GLControl_Paint;
            glControl.Dock = DockStyle.Fill;
            var windowsFormsHost = sender as WindowsFormsHost;
            if (windowsFormsHost != null) windowsFormsHost.Child = glControl;
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            glControl.MakeCurrent();
            GL.ClearColor(0.933f, 0.933f, 0.933f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Finish();
            glControl.SwapBuffers();

            glControl.Invalidate();
        }

        private void WinFormsHost_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
    }
}
