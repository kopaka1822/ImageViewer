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

            Debug.Assert(ViewModel != null);
            DataContext = ViewModel;

            Width = Properties.Settings.Default.WindowSizeX;
            Height = Properties.Settings.Default.WindowSizeY;
        }



        public void ImportImages(string[] files)
        {
            foreach (var file in files)
            {
                ViewModel.ImportImage(file);
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            ViewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
