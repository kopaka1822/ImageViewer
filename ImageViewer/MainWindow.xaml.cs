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
using OpenTK.WPF;

using OpenTK.Graphics.OpenGL4;
using ClearBufferMask = OpenTK.Graphics.OpenGL4.ClearBufferMask;

namespace ImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static float r = 0.0f;
        private float red;
        public MainWindow()
        {
            InitializeComponent();
            red = r;
            r += 1.0f;
        }

        private void OtkWpfControl_OnInitialized(object sender, EventArgs e)
        {
        }

        private void OtkWpfControl_OnResized(object sender, EventArgs e)
        {
        }

        private void OtkWpfControl_OnOpenGLDraw(object sender, OtkWpfControl.OpenGLDrawEventArgs e)
        {
            var ctrl = sender as OpenTK.WPF.OtkWpfControl;
            if (ctrl != null)
            {
                GL.ClearColor(red,red,red, 1.0f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                
                e.Redrawn = true;
            }
        }
    }
}
