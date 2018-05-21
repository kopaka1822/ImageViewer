using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TextureViewer.Commands
{
    public class AddFilterCommand : ICommand
    {
        private readonly Models.Models models;

        public AddFilterCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
