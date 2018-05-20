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
        public ExportModel Model { get; }

        public ExportDialog(Models.Models models, string filename, PixelFormat defaultPixelFormat, ExportModel.FileFormat format)
        {
            Model = new ExportModel(filename, defaultPixelFormat, format);
            DataContext = new ExportViewModel(models, Model);

            InitializeComponent();
        }
    }
}
