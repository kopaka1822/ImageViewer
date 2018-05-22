using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TextureViewer.Commands
{
    public class OpenImageCommand : ICommand
    {
        private readonly Models.Models models;
        private readonly ImportImageCommand import;

        public OpenImageCommand(Models.Models models, ImportImageCommand import)
        {
            this.models = models;
            this.import = import;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            // import image if nothing was imported yet
            if (models.Images.NumImages == 0)
            {
                import.Execute(parameter);
                return;
            }

            var files = Utility.Utility.ShowImportImageDialog(models.App.Window);
            if (files == null) return;

            var wnd = models.App.App.SpawnWindow();
            wnd.ImportImages(files);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
