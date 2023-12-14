using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageViewer.Models;

namespace ImageViewer.Commands.Helper
{
    /// <inheritdoc />
    /// <summary>
    /// parameterless command
    /// </summary>
    public abstract class Command : ICommand
    {
        protected readonly ModelsEx models;

        protected Command(ModelsEx models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter) => CanExecute();

        public abstract bool CanExecute();

        public async void Execute(object parameter)
        {
            try
            {
                await ExecuteAsync();
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }

        public virtual void Execute() {}

        public virtual Task ExecuteAsync()
        {
            Execute();
            return Task.CompletedTask;
        }

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
        protected readonly ModelsEx models;

        protected Command(ModelsEx models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return CanExecute((T)parameter);
        }

        public abstract bool CanExecute(T parameter);

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

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
