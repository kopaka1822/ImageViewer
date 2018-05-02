using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Commands;
using TextureViewer.Models;

namespace TextureViewer.ModelViews
{
    /// <summary>
    /// has all interaction logic for the main window and all models
    /// </summary>
    public class WindowViewModel
    {
        private readonly ImagesModel imagesModel = new ImagesModel();
        private readonly DisplayModel displayModel;
        private readonly ImagesViewModel imagesViewModel;
        private readonly App app;
        private readonly MainWindow window;

        public WindowViewModel(App app, MainWindow window)
        {
            this.app = app;
            this.window = window;
            displayModel = new DisplayModel(imagesModel);
            imagesViewModel = new ImagesViewModel(imagesModel);
            Display = new DisplayViewModel(displayModel, imagesModel);
            ImportCommand = new ImportImageCommand(imagesViewModel);
            ResizeCommand = new ResizeWindowCommand(window);
        }

        public ICommand ImportCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResizeCommand { get; }

        public ObservableCollection<string> ImageList { get; } = new ObservableCollection<string>() {"hello", "there"};

        public DisplayViewModel Display { get; }
    }
}
