using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImageViewer.Commands
{
    /// <summary>
    /// commands that can always be executed and takes parameter T
    /// </summary>
    /// <typeparam name="T">action argument type</typeparam>
    public class ActionCommand<T> : ICommand
    {
        private readonly Action<T> m_action;

        public ActionCommand(Action<T> action)
        {
            m_action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            m_action((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }

    /// <summary>
    /// command that can always be executed and takes no parameters
    /// </summary>
    public class ActionCommand : ICommand
    {
        private readonly Action m_action;

        public ActionCommand(Action action)
        {
            m_action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            m_action();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
