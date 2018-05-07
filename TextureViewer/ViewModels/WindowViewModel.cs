using System.Collections.ObjectModel;
using System.Windows.Input;
using TextureViewer.Commands;
using TextureViewer.Controller;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    /// <summary>
    /// has all interaction logic for the main window and all models
    /// </summary>
    public class WindowViewModel
    {
        private readonly Models.Models models;
        private readonly PaintController paintController;

        public WindowViewModel(App app, MainWindow window)
        {
            this.models = new Models.Models(app, window);

            // controller
            this.paintController = new PaintController(models);
            
            // model views
            Images = new ImagesViewModel(models);
            Display = new DisplayViewModel(models);
            Equations = new EquationsViewModel(models);
            Progress = new ProgressViewModel(models);

            // commands
            ImportCommand = new ImportImageCommand(models);
            ResizeCommand = new ResizeWindowCommand(models);
        }

        // view models
        public DisplayViewModel Display { get; }
        public ImagesViewModel Images { get; }
        public EquationsViewModel Equations { get; }
        public ProgressViewModel Progress { get; }

        // commands
        public ICommand ImportCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResizeCommand { get; }
    }
}
