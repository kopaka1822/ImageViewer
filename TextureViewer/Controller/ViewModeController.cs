using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Controller.TextureViews;
using TextureViewer.Models;

namespace TextureViewer.Controller
{
    public class ViewModeController
    {
        private Models.Models models;
        private ITextureView currentView = new EmptyView();

        public ViewModeController(Models.Models models)
        {
            this.models = models;
            models.Display.PropertyChanged += ViewModeModelOnPropertyChanged;
        }

        private void ViewModeModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.ActiveView):
                    switch (models.Display.ActiveView)
                    {
                        case DisplayModel.ViewMode.Empty:
                            currentView = new EmptyView();
                            break;
                        case DisplayModel.ViewMode.Single:
                            currentView = new SingleTextureView(models);
                            break;
                        case DisplayModel.ViewMode.CubeMap:
                            break;
                        case DisplayModel.ViewMode.Polar:
                            break;
                        case DisplayModel.ViewMode.CubeCrossView:
                            break;
                    }
                    break;
            }
        }

        public void Paint()
        {
            currentView.Draw();
        }
    }
}
