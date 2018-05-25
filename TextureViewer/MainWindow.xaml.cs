using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

        public void ImportImages(string[] files)
        {
            foreach (var file in files)
            {
                ViewModel.ImportImage(file);
            }
        }

        /// <summary>
        /// helper to update a text box if enter is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateOnEnter(object sender, KeyEventArgs e)
        {
            // only update on enter
            if (e.Key != Key.Enter) return;

            var box = (DependencyObject) sender;
            var prop = TextBox.TextProperty;

            var binding = BindingOperations.GetBindingExpression(box, prop);
            binding?.UpdateSource();
            Keyboard.ClearFocus();
        }
    }
}
