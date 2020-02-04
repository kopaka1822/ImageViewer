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

namespace ImageViewer.Controller
{
    /// <summary>
    /// sets the appropriate crop rectangle
    /// </summary>
    public class CropController
    {
        private readonly ModelsEx models;

        public CropController(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
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
