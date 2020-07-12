using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageViewer.Models.Settings
{
    public class ExportConfig
    {
        public bool ShowCropBox { get; set; }

        public float CropStartX { get; set; }
        public float CropStartY { get; set; }
        public float CropStartZ { get; set; }

        public float CropEndX { get; set; }
        public float CropEndY { get; set; }
        public float CropEndZ { get; set; }

        public bool UseCropping { get; set; }

        public ZoomBoxConfig ZoomBox { get; set; } = new ZoomBoxConfig();

        public static ExportConfig LoadFromModels(ModelsEx models)
        {
            var res = new ExportConfig();
            res.ShowCropBox = models.Display.ShowCropRectangle;
            res.CropStartX = models.ExportConfig.CropStart.X;
            res.CropStartY = models.ExportConfig.CropStart.Y;
            res.CropStartZ = models.ExportConfig.CropStart.Z;
            res.CropEndX = models.ExportConfig.CropEnd.X;
            res.CropEndY = models.ExportConfig.CropEnd.Y;
            res.CropEndZ = models.ExportConfig.CropEnd.Z;
            res.UseCropping = models.ExportConfig.UseCropping;
            res.ZoomBox = ZoomBoxConfig.LoadFromModels(models);

            return res;
        }

        public void ApplyToModels(ModelsEx models)
        {
            models.Display.ShowCropRectangle = ShowCropBox;
            models.ExportConfig.CropStart = new Float3(CropStartX, CropStartY, CropStartZ);
            models.ExportConfig.CropEnd = new Float3(CropEndX, CropEndY, CropEndZ);
            models.ExportConfig.UseCropping = UseCropping;
            ZoomBox?.ApplyToModels(models);
        }
    }
}
