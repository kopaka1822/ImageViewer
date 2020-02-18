using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageViewer.Commands.Helper
{
    /// <summary>
    /// adapter from ActionCommand with parameter to parameter less command
    /// </summary>
    public class ParameterCommandAdapter<T> : ICommand
    {
        private readonly Command<T> command;
        private readonly T param;

        public ParameterCommandAdapter(Command<T> command, T param)
        {
            this.command = command;
            this.param = param;
        }

        public bool CanExecute(object parameter)
        {
            return command.CanExecute(param);
        }

        public void Execute(object parameter)
        {
            command.Execute(param);
        }

        public event EventHandler CanExecuteChanged
        {
            add => command.CanExecuteChanged += value;
            remove => command.CanExecuteChanged -= value;
        }
    }
}
