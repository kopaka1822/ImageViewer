using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TextureViewer.Views;

namespace TextureViewer.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly string path;

        public HelpCommand(string path)
        {
            this.path = path;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            new HelpDialog(path).Show();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
