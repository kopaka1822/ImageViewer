using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Models.Display;

namespace ImageViewer.Controller
{
    /// <summary>
    /// sets the appropriate crop rectangle
    /// </summary>
    public class CropController
    {
        // overlay that synchronizes export and display model settings with the crop overlay
        private class CropOverlay : ImageFramework.Model.Overlay.CropOverlay
        {
            private readonly ModelsEx models;

            public CropOverlay(ModelsEx models) : base(models)
            {
                this.models = models;
                models.Display.PropertyChanged += DisplayOnPropertyChanged;
                models.Export.PropertyChanged += ExportOnPropertyChanged;

                Start = models.Export.CropStart;
                End = models.Export.CropEnd;
                Layer = models.Export.Layer;
                EvaluateIsEnabled();
            }

            private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(models.Export.UseCropping):
                        EvaluateIsEnabled();
                        break;
                    case nameof(models.Export.CropStart):
                        Start = models.Export.CropStart;
                        break;
                    case nameof(models.Export.CropEnd):
                        End = models.Export.CropEnd;
                        break;
                    case nameof(models.Export.Layer):
                        Layer = models.Export.Layer;
                        break;
                }

            }

            private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case nameof(DisplayModel.IsExporting):
                    case nameof(DisplayModel.ShowCropRectangle):
                        EvaluateIsEnabled();
                        break;
                }
            }

            private void EvaluateIsEnabled()
            {
                IsEnabled = models.Export.UseCropping &&
                            (models.Display.IsExporting || models.Display.ShowCropRectangle);
            }
        }

        private readonly ModelsEx models;

        public CropController(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
            this.models.Overlay.Overlays.Add(new CropOverlay(models));
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if (models.Images.PrevNumImages == 0)
                        AdjustCroppingRect();
                    break;
            }
        }

        private void AdjustCroppingRect()
        {
            // reset cropping rect
            models.Export.CropStart = Float3.Zero;
            models.Export.CropEnd = Float3.One;
        }
    }
}
