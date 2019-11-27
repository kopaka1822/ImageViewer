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
            this.models.Export.PropertyChanged += ExportOnPropertyChanged;
        }

        private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ExportModel.Mipmap):
                    AdjustCroppingRect(false);
                    break;
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if (models.Images.PrevNumImages == 0)
                        AdjustCroppingRect(true);
                    break;
            }
        }

        private void AdjustCroppingRect(bool overwrite)
        {
            if (models.Images.NumImages == 0)
                return; // no need to adjust

            var mip = models.Export.Mipmap;
            if (models.Images.NumMipmaps == 1) // => all mipmaps (-1) is equal to the first mipmap
                mip = 0;

            // all mipmaps does not support cropping
            if (mip == -1) return;

            // no valid mipmap set yet
            if (mip >= models.Images.NumMipmaps) return;

            var maxX = models.Images.GetWidth(mip) - 1;
            var maxY = models.Images.GetHeight(mip) - 1;
            var maxZ = models.Images.GetDepth(mip) - 1;

            if (overwrite)
            {
                models.Export.CropStartX = 0;
                models.Export.CropStartY = 0;
                models.Export.CropStartZ = 0;
                models.Export.CropEndX = maxX;
                models.Export.CropEndY = maxY;
                models.Export.CropEndZ = maxZ;
            }
            else
            {
                models.Export.CropStartX = Utility.Clamp(models.Export.CropStartX, 0, maxX);
                models.Export.CropStartY = Utility.Clamp(models.Export.CropStartY, 0, maxY);
                models.Export.CropStartZ = Utility.Clamp(models.Export.CropStartZ, 0, maxZ);

                models.Export.CropEndX = Utility.Clamp(models.Export.CropEndX, models.Export.CropStartX, maxX);
                models.Export.CropEndY = Utility.Clamp(models.Export.CropEndY, models.Export.CropStartY, maxY);
                models.Export.CropEndZ = Utility.Clamp(models.Export.CropEndZ, models.Export.CropStartZ, maxZ);
            }
        }
    }
}
