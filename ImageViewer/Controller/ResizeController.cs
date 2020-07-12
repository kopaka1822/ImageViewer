using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageViewer.Models;

namespace ImageViewer.Controller
{
    /// <summary>
    /// forward client resize events
    /// </summary>
    public class ResizeController
    {
        private readonly ModelsEx models;

        public ResizeController(ModelsEx models)
        {
            this.models = models;
            this.models.Window.PropertyChanged += WindowOnPropertyChanged;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.Size):
                    if (models.Images.NumImages == 0) return;

                    // adjust zoom to fit into screen
                    var imgSize = models.Images.Size;
                    var clientSize = models.Window.ClientSize;

                    var scaleX = (float)clientSize.Width / imgSize.Width;
                    var scaleY = (float)clientSize.Height / imgSize.Height;

                    models.Display.SetClosestZoomPoint(Math.Min(scaleX, scaleY));
                    break;
            }
        }

        private void WindowOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(WindowModel.ClientSize):
                    models.Display.RecomputeAspectRatio(models.Window.ClientSize);
                    break;
            }
        }
    }
}
