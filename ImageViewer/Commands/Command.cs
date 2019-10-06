using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageViewer.Commands
{
    /// <inheritdoc />
    /// <summary>
    /// parameterless command
    /// </summary>
    public abstract class Command : ICommand
    {
        public bool CanExecute(object parameter) => CanExecute();

        public abstract bool CanExecute();

        public void Execute(object parameter) => Execute();

        public abstract bool Execute();

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// command with parameter type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Command<T> : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        public abstract bool CanExecute(T parameter);

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public abstract void Execute(T parameter);

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
