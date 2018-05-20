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

        public ImportImageCommand(Models.Models models)
        {
            this.models = models;
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
            var files = Utility.Utility.ShowImportImageDialog();
            if (files == null) return;

            foreach (var filename in files)
            {
                // load image
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
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
