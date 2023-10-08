using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using ImageViewer.Models;

namespace ImageViewer.Controller.Overlays
{
    internal class CropBoxOverlay : GenericBoxOverlay
    {
        public CropBoxOverlay(ModelsEx models) : base(models)
        {

        }

        protected override void OnFinished(Float2 start, Float2 end)
        {
            models.Display.ShowCropRectangle = true;
            models.ExportConfig.UseCropping = true;
            models.ExportConfig.CropStart = new Float3(start, 0.0f);
            models.ExportConfig.CropEnd = new Float3(end, 1.0f);
        }
    }
}
