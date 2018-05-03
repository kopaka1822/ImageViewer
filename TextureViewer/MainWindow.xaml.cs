using System;
using System.Windows;
using TextureViewer.Controller;
using TextureViewer.ViewModels;

namespace TextureViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private App parent;
        private OpenGlController glController;

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
            glController = new OpenGlController(this);
        }
    }
}
