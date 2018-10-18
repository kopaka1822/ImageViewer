using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace TextureViewer.Commands
{
    public class ResizeWindowCommand : ICommand
    {
        private readonly Models.Models models;

        public ResizeWindowCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            // update settings
            if (models.App.Window.WindowState == WindowState.Maximized)
            {
                Properties.Settings.Default.IsMaximized = true;
            }
            else if(models.App.Window.WindowState != WindowState.Minimized)
            {
                Properties.Settings.Default.WindowSizeX = (int)models.App.Window.Width;
                Properties.Settings.Default.WindowSizeY = (int)models.App.Window.Height;
                Properties.Settings.Default.IsMaximized = false;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
