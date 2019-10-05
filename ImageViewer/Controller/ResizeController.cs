using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
