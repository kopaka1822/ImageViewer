using System;
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
            Filter = new FiltersViewModel(models);
            Statistics = new StatisticsViewModel(models);

            // commands
            var import = new ImportImageCommand(models);
            ImportCommand = import;
            ResizeCommand = new ResizeWindowCommand(models);
            OpenCommand = new OpenImageCommand(models, import);
            ExportCommand = new ExportImageCommand(models);
            AddFilterCommand = new AddFilterCommand(models, Filter);
            ShowPixelDisplayCommand = new ShowPixelDialogCommand(models);

            window.KeyUp += WindowOnKeyUp;
        }

        private void WindowOnKeyUp(object sender, KeyEventArgs e)
        {
            if (Filter.HasKeyToInvoke(e.Key))
            {
                // invoke the key
                Filter.InvokeKey(e.Key);

                if(Filter.ApplyCommand.CanExecute(null))
                    Filter.ApplyCommand.Execute(null);
            }
        }

        /// <summary>
        /// tries to import an image. Displays an error on failure
        /// </summary>
        /// <param name="filename"></param>
        public void ImportImage(string filename)
        {
            try
            {
                var imgs = ImageLoader.LoadImage(filename);
                models.Images.AddImages(imgs);
            }
            catch (Exception e)
            {
                App.ShowErrorDialog(models.App.Window, e.Message);
            }
        }

        // view models
        public DisplayViewModel Display { get; }
        public ImagesViewModel Images { get; }
        public EquationsViewModel Equations { get; }
        public ProgressViewModel Progress { get; }
        public FiltersViewModel Filter { get; }
        public StatisticsViewModel Statistics { get; }

        // commands
        public ICommand ImportCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResizeCommand { get; }
        public ICommand AddFilterCommand { get; }
        public ICommand ShowPixelDisplayCommand { get; }
    }
}
