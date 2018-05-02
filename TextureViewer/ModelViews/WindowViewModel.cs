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
        private readonly DisplayViewModel displayViewModel;

        public WindowViewModel()
        {
            displayModel = new DisplayModel(imagesModel);
            imagesViewModel = new ImagesViewModel(imagesModel);
            displayViewModel = new DisplayViewModel(displayModel, imagesModel);
            ImportCommand = new ImportImageCommand(imagesViewModel);
        }

        public ICommand ImportCommand { get; }

        public ObservableCollection<string> ImageList { get; } = new ObservableCollection<string>() {"hello", "there"};

        public DisplayViewModel Display => displayViewModel;
    }
}
