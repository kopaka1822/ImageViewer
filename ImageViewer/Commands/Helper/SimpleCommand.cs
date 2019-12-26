using System;
using System.Windows.Input;

namespace ImageViewer.Commands.Helper
{
    /// <summary>
    /// a simple command can always be executed
    /// </summary>
    public abstract class SimpleCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Execute();
        }

        public abstract void Execute();

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// command with parameter type that can always be executed
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SimpleCommand<T> : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Execute((T)parameter);
        }

        public abstract void Execute(T parameter);

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
