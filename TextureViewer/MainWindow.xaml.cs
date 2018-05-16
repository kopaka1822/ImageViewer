using System;
using System.Diagnostics;
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
        public WindowViewModel ViewModel { get; private set; }

        public MainWindow(App parent)
        {
            this.parent = parent;

            InitializeComponent();

            Debug.Assert(ViewModel != null);
            DataContext = ViewModel;

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
            try
            {
                // initialize data models
                ViewModel = new WindowViewModel(parent, this);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }

        }
    }
}
