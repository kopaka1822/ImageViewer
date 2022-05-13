using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.ViewModels.Display;
using ImageViewer.Views.Display;

namespace ImageViewer.Controller.Overlays
{
    public class MovieOverlay : IDisplayOverlay
    {
        private MovieView view;
        private MovieViewModel viewModel;

        public MovieOverlay(ModelsEx models)
        {
            view = new MovieView();
            viewModel = new MovieViewModel(models);
            view.DataContext = viewModel;
        }

        public void Dispose()
        {
            viewModel?.Dispose();
        }

        public void MouseMove(Size3 texel)
        {
            
        }

        public void MouseClick(MouseButton button, bool down, Size3 texel)
        {
            
        }

        public bool OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.OemComma:
                    viewModel.PreviousFrame();
                    break;
                case Key.OemPeriod:
                    viewModel.NextFrame();
                    break;
                case Key.Space:
                    viewModel.PlayPause();
                    break;
                default:
                    return false;
            }
            return true;
        }

        public UIElement View => view;
    }
}
