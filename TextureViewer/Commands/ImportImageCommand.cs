using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.ViewModels;

namespace TextureViewer.Commands
{
    public class ImportImageCommand : ICommand
    {
        private readonly ImagesViewModel viewModel;

        public ImportImageCommand(ImagesViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            viewModel.ImportImage();
        }

        public event EventHandler CanExecuteChanged;
    }
}
