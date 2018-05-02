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
using TextureViewer.Models;
using TextureViewer.ViewModels;
using DragEventArgs = System.Windows.Forms.DragEventArgs;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private App parent;
        private OpenGlViewModel glModelView;

        public MainWindow(App parent)
        {
            this.parent = parent;
            InitializeComponent();
            DataContext = new WindowViewModel(parent, this);

            Width = Properties.Settings.Default.WindowSizeX;
            Height = Properties.Settings.Default.WindowSizeY;
        }

        /// <summary>
        /// intializes the opengl frame (happens before the data context binding)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenGlHost_OnInitialized(object sender, EventArgs e)
        {
            glModelView = new OpenGlViewModel(this);
        }
    }
}
