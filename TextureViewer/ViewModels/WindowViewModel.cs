using System;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using TextureViewer.Commands;
using TextureViewer.Controller;
using TextureViewer.Models;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

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
            this.models = new Models.Models(app, window, this);

            // controller
            this.paintController = new PaintController(models);
            
            // model views
            Images = new ImagesViewModel(models, this);
            Display = new DisplayViewModel(models);
            Equations = new EquationsViewModel(models);
            Progress = new ProgressViewModel(models);
            Filter = new FiltersViewModel(models);
            Statistics = new StatisticsViewModel(models);

            // commands
            var import = new ImportImageCommand(models, this);
            ImportCommand = import;
            ResizeCommand = new ResizeWindowCommand(models);
            OpenCommand = new OpenImageCommand(models, import);
            ExportCommand = new ExportImageCommand(models);
            AddFilterCommand = new AddFilterCommand(models, Filter);
            ShowPixelDisplayCommand = new ShowPixelDialogCommand(models);
            ShowPixelColorCommand = new ShowPixelColorCommand(models);

            window.KeyUp += WindowOnKeyUp;
            models.GlContext.GlControl.DragDrop += GlControlOnDragDrop;

            HelpAboutCommand = new HelpCommand("help\\about.md");
            HelpEquationCommand = new HelpCommand("help\\equation.md");
            HelpFilterManualCommand = new HelpCommand("help\\filter_manual.md");
        }

        public void Dispose()
        {
            models.Dispose();
        }

        private void GlControlOnDragDrop(object sender, DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])args.Data.GetData(DataFormats.FileDrop);

                if (files != null)
                    foreach (var file in files)
                        ImportImage(file);
            }
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
        /// tries to import an image. Displays an error on failure. Returns true on success
        /// </summary>
        /// <param name="filename"></param>
        public bool ImportImage(string filename)
        {
            // maximum amount of images reached?
            if(models.Images.NumImages == models.GlContext.MaxTextureUnits)
            {
                App.ShowErrorDialog(models.App.Window, $"Maximum texture units reached. This GPU only supports {models.GlContext.MaxTextureUnits} units");
                return false;
            }

            try
            {
                var imgs = ImageLoader.LoadImage(filename);
                models.Images.AddImages(imgs);
                return true;
            }
            catch (Exception e)
            {
                App.ShowErrorDialog(models.App.Window, e.Message);
            }
            return false;
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
        public ICommand ShowPixelColorCommand { get; }

        public ICommand HelpAboutCommand { get; }
        public ICommand HelpEquationCommand { get; }
        public ICommand HelpFilterManualCommand { get; }
    }
}
