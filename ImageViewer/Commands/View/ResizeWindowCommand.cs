using System.Windows;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.View
{
    public class ResizeWindowCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ResizeWindowCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            var wnd = models.Window.Window;
            // update settings
            if (wnd.WindowState == WindowState.Maximized)
            {
                models.Settings.IsMaximized = true;
            }
            else if (wnd.WindowState != WindowState.Minimized)
            {
                models.Settings.IsMaximized = false;
                models.Settings.WindowWidth = (int) wnd.Width;
                models.Settings.WindowHeight = (int) wnd.Height;
            }
        }
    }
}
