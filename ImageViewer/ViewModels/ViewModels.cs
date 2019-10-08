using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageViewer.Commands;
using ImageViewer.Models;

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
        public ViewModels(ModelsEx models)
        {
            this.models = models;

            Display = new DisplayViewModel(models);
            Progress = new ProgressViewModel(models);
            Images = new ImagesViewModel(models);
            Equations = new EquationsViewModel(models);
            Filter = new FiltersViewModel(models);
            Statistics = new StatisticsViewModel(models);

            // commands
            OpenCommand = new OpenCommand(models);
            ImportCommand = new ImportCommand(models);
            ImportEquationImageCommand = new ImportEquationImageCommand(models);
            ExportCommand = new ExportCommand(models);

            ShowPixelDisplayCommand = new ShowPixelDisplayCommand(models);
            GenerateMipmapsCommand = new GenerateMipmapsCommand(models);
            DeleteMipmapsCommand = new DeleteMipmapsCommand(models);
            HelpCommand = new HelpDialogCommand(models);

            ResizeWindow = new ResizeWindowCommand(models);
            SetThemeCommand = new SetThemeCommand(models);

            AddFilterCommand = new AddFilterCommand(models, Filter);
        }

        public void Dispose()
        {
            models?.Dispose();
        }

        public ICommand ResizeWindow { get; }

        public ICommand SetThemeCommand { get; }

        public ICommand OpenCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ImportEquationImageCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand ShowPixelDisplayCommand { get; }

        public ICommand GenerateMipmapsCommand { get; }
        public ICommand DeleteMipmapsCommand { get; }

        public ICommand HelpCommand { get; }

        public ICommand AddFilterCommand { get; }
    }
}
