using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageViewer.Commands;
using ImageViewer.Commands.Export;
using ImageViewer.Commands.Import;
using ImageViewer.Commands.Tools;
using ImageViewer.Commands.View;
using ImageViewer.Models;
using ImageViewer.ViewModels.Display;
using ImageViewer.ViewModels.Image;
using ImageViewer.ViewModels.Statistics;

namespace ImageViewer.ViewModels
{
    public class ViewModels : IDisposable
    {
        private readonly ModelsEx models;

        public DisplayViewModel Display { get; }
        public ProgressViewModel Progress { get; }

        public ImagesViewModel Images { get; }
        public FiltersViewModel Filter { get; }

        public EquationsViewModel Equations { get; }

        public StatisticsViewModel Statistics { get; }

        public ScalingViewModel Scale { get; }
        public ViewModels(ModelsEx models)
        {
            this.models = models;

            // view models
            Display = new DisplayViewModel(models);
            Progress = new ProgressViewModel(models);
            Images = new ImagesViewModel(models);
            Equations = new EquationsViewModel(models);
            Filter = new FiltersViewModel(models);
            Statistics = new StatisticsViewModel(models);
            Scale = new ScalingViewModel(models);

            // commands
            OpenCommand = new OpenCommand(models);
            ImportCommand = new ImportCommand(models);
            ImportEquationImageCommand = new ImportEquationImageCommand(models);
            ExportCommand = new ExportCommand(models);
            ReloadImagesCommand = new ReloadImagesCommand(models);

            ShowPixelDisplayCommand = new ShowPixelDisplayCommand(models);
            ShowPixelColorCommand = new ShowPixelColorCommand(models);
            ShowScaleCommand = new ShowScaleCommand(models);
            GenerateMipmapsCommand = new GenerateMipmapsCommand(models);
            DeleteMipmapsCommand = new DeleteMipmapsCommand(models);
            HelpCommand = new HelpDialogCommand(models);
            GifExportCommand = new GifExportCommand(models);
            ImportArrayCommand = new ImportArrayCommand(models);
            LatLongToCubemapCommand = new LatLongToCubemapCommand(models);
            CubemapToLatLongCommand = new CubemapToLatLongCommand(models);
            SelectNaNColorCommand = new SelectNaNColorCommand(models);

            ResizeCommand = new ResizeWindowCommand(models);
            SetThemeCommand = new SetThemeCommand(models);
            StartZoomboxCommand = new StartZoomboxCommand(models);

            AddFilterCommand = new AddFilterCommand(models, Filter);

            // key input
            models.Window.Window.KeyDown += WindowOnKeyDown;
        }

        private void WindowOnKeyDown(object sender, KeyEventArgs e)
        {
            if (Filter.HasKeyToInvoke(e.Key))
            {
                // invoke the key
                Filter.InvokeKey(e.Key);

                if (Filter.ApplyCommand.CanExecute(null))
                    Filter.ApplyCommand.Execute(null);

                return;
            }

            if(Display.HasKeyToInvoke(e.Key))
                Display.InvokeKey(e.Key);
        }

        public void Dispose()
        {
            models?.Dispose();
        }

        public ICommand ResizeCommand { get; }

        public ICommand SetThemeCommand { get; }

        public ICommand OpenCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ImportEquationImageCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand ShowPixelDisplayCommand { get; }

        public ICommand ShowPixelColorCommand { get; }

        public ICommand ShowScaleCommand { get; }

        public ICommand GenerateMipmapsCommand { get; }
        public ICommand DeleteMipmapsCommand { get; }

        public ICommand HelpCommand { get; }

        public ICommand AddFilterCommand { get; }

        public ICommand GifExportCommand { get; }

        public ICommand ImportArrayCommand { get; }

        public ICommand LatLongToCubemapCommand { get; }
        public ICommand CubemapToLatLongCommand { get; }

        public ICommand SelectNaNColorCommand { get; }

        public ICommand ReloadImagesCommand { get; }

        public ICommand StartZoomboxCommand { get; }
    }
}
