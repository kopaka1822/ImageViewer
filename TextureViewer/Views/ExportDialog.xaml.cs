using System;
using System.Windows;
using TextureViewer.Models.Dialog;
using TextureViewer.ViewModels.Dialog;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace TextureViewer.Views
{
    /// <summary>
    /// Interaction logic for ExportDialog.xaml
    /// </summary>
    public partial class ExportDialog : Window
    {
        private readonly ExportViewModel viewModel;

        public ExportDialog(Models.Models models, string filename, PixelFormat defaultPixelFormat, ExportModel.FileFormat format)
        {
            models.Export.Init(filename, defaultPixelFormat, format);
            viewModel = new ExportViewModel(models);
            DataContext = viewModel;

            InitializeComponent();
        }

        private void ButtonExport_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            viewModel.Dispose();
            base.OnClosed(e);
        }
    }
}
