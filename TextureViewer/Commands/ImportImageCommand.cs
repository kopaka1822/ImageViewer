using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TextureViewer.Models;
using TextureViewer.ViewModels;

namespace TextureViewer.Commands
{
    public class ImportImageCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly WindowViewModel viewModel;

        public ImportImageCommand(Models.Models models, WindowViewModel viewModel)
        {
            this.models = models;
            this.viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// opens a file dialoge to import images
        /// </summary>
        public void Execute(object parameter)
        {
            var files = Utility.Utility.ShowImportImageDialog(models.App.Window);
            if (files == null) return;

            foreach (var filename in files)
            {
                viewModel.ImportImage(filename);
            }  
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
