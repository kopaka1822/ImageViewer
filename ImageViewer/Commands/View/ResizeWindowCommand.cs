using System.Windows;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.View
{
    public class ResizeWindowCommand : SimpleCommand
    {

        public ResizeWindowCommand(ModelsEx models) : base(models)
        {
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
