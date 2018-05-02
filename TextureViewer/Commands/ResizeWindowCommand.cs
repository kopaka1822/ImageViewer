using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TextureViewer.Commands
{
    public class ResizeWindowCommand : ICommand
    {
        private MainWindow window;

        public ResizeWindowCommand(MainWindow window)
        {
            this.window = window;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            // update settings
            Properties.Settings.Default.WindowSizeX = (int) window.Width;
            Properties.Settings.Default.WindowSizeY = (int) window.Height;
        }

        public event EventHandler CanExecuteChanged;
    }
}
