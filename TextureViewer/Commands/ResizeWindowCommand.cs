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
            Properties.Settings.Default.WindowSizeX = (int) models.App.Window.Width;
            Properties.Settings.Default.WindowSizeY = (int)models.App.Window.Height;
        }

        public event EventHandler CanExecuteChanged;
    }
}
