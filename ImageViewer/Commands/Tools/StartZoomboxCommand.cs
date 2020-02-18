using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Controller.Overlays;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class StartZoomboxCommand : Command
    {
        private readonly ModelsEx models;

        public StartZoomboxCommand(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(ImagesModel.NumImages))
                OnCanExecuteChanged();
        }

        public override bool CanExecute()
        {
            return models.Images.NumImages != 0;
        }

        public override void Execute()
        {
            var overlay = new ZoomBoxOverlay(models);
            models.Display.ActiveOverlay = overlay;
        }
    }
}
