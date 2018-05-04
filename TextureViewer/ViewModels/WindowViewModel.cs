using System.Collections.ObjectModel;
using System.Windows.Input;
using TextureViewer.Commands;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    /// <summary>
    /// has all interaction logic for the main window and all models
    /// </summary>
    public class WindowViewModel
    {
        private readonly Models.Models models;
        private readonly App app;
        private readonly MainWindow window;

        public WindowViewModel(App app, MainWindow window)
        {
            this.app = app;
            this.window = window;
            this.models = new Models.Models(window);
            
            
            Images = new ImagesViewModel(models);
            Display = new DisplayViewModel(models);
            ImportCommand = new ImportImageCommand(models.Images, this.window);
            ResizeCommand = new ResizeWindowCommand(window);
        }

        public ICommand ImportCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResizeCommand { get; }

        public DisplayViewModel Display { get; }
        public ImagesViewModel Images { get; }
    }
}
