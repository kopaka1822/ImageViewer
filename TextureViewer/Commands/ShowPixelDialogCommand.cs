using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Views;

namespace TextureViewer.Commands
{
    public class ShowPixelDialogCommand : ICommand
    {
        private readonly Models.Models models;

        public ShowPixelDialogCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var dia = new PixelDisplayDialog(models);
            dia.Owner = models.App.Window;
            dia.ShowDialog();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
