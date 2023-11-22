using System;
using System.Windows.Input;
using ImageViewer.Models;

namespace ImageViewer.Commands.Helper
{
    /// <summary>
    /// a simple command can always be executed
    /// </summary>
    public abstract class SimpleCommand : ICommand
    {
        protected readonly ModelsEx models;

        protected SimpleCommand(ModelsEx models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                Execute();
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
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
        protected readonly ModelsEx models;

        protected SimpleCommand(ModelsEx models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            try
            {
                Execute((T)parameter);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }

        public abstract void Execute(T parameter);

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
