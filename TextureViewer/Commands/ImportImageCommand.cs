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
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = Properties.Settings.Default.ImagePath
            };

            if (ofd.ShowDialog() != true) return;

            // set new image path in settings
            Properties.Settings.Default.ImagePath = System.IO.Path.GetDirectoryName(ofd.FileName);

            foreach (var filename in ofd.FileNames)
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

        public event EventHandler CanExecuteChanged;
    }
}
