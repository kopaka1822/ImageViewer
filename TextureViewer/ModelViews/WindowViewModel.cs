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
        private readonly Images imagesModel = new Images();
        private readonly ImagesViewModel imagesViewModel;

        public WindowViewModel()
        {
            imagesViewModel = new ImagesViewModel(imagesModel);
            ImportCommand = new ImportImageCommand(imagesViewModel);
        }

        public ICommand ImportCommand { get; }

        public ObservableCollection<string> ImageList { get; } = new ObservableCollection<string>() {"hello", "there"};
    }
}
