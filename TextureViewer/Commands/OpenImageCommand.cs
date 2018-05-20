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

        public OpenImageCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var files = Utility.Utility.ShowImportImageDialog();
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
