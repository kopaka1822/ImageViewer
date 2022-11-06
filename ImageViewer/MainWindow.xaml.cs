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
using ImageFramework.DirectX;
using ImageViewer.Controller;
using ImageViewer.DirectX;
using ImageViewer.Models;
using ImageViewer.Models.Settings;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ModelsEx models;
        public ViewModels.ViewModels ViewModel;


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                models = new ModelsEx(this);
                ViewModel = new ViewModels.ViewModels(models);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            DataContext = ViewModel;
            Width = models.Settings.WindowWidth;
            Height = models.Settings.WindowHeight;
            if (models.Settings.IsMaximized)
                WindowState = WindowState.Maximized;

            // handle startup arguments
            if (App.StartupArgs.Length == 0) return;

            LoadStartupArgsAsync();
        }

        private async void LoadStartupArgsAsync()
        {
            try
            {
                foreach (var arg in App.StartupArgs)
                {
                    await models.Import.ImportFileAsync(arg);
                }
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ViewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}
