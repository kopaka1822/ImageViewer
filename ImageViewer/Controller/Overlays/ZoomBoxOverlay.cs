using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Controller.Overlays
{
    public class ZoomBoxOverlay : CropOverlay, IDisplayOverlay
    {
        private readonly ModelsEx models;
        private bool isDisposed = false;
        private Float3? firstPoint;

        public ZoomBoxOverlay(ModelsEx models) : base(models)
        {
            this.models = models;
            IsEnabled = true;
            Layer = models.Display.ActiveLayer;
            models.Overlay.Overlays.Add(this);
        }

        public void MouseMove(Size3 texel)
        {
            Layer = models.Display.ActiveLayer;
            if (firstPoint == null) return;
            SetCropRect(GetCurrentCoords(texel));
        }

        public void MouseClick(MouseButton button, bool down, Size3 texel)
        {
            if(down) return;
            if (button != MouseButton.Left) return;
            if (firstPoint == null)
            {
                firstPoint = GetCurrentCoords(texel);
                SetCropRect(GetCurrentCoords(texel));
            }
            else
            {
                SetCropRect(GetCurrentCoords(texel));
                var dia = new ZoomBoxDialog(new Color(1.0f, 0.0f, 0.0f), 3);
                if (models.Window.ShowDialog(dia) == true)
                {
                    var start = Start.Value.ToPixels(models.Images.Size);
                    var end = End.Value.ToPixels(models.Images.Size);

                    // add zoom box
                    var box = new BoxOverlay.Box
                    {
                        Border = dia.BorderSize,
                        Color = dia.Color,
                        StartX = start.X,
                        EndX = end.X,
                        StartY = start.Y,
                        EndY = end.Y
                    };
                    models.ZoomBox.Boxes.Add(box);
                }

                models.Display.ActiveOverlay = null;
            }
        }

        private Float3 GetCurrentCoords(Size3 texel)
        {
            return texel.ToCoords(models.Images.GetSize(models.Display.ActiveMipmap));
        }

        private void SetCropRect(Float3 secondPoint)
        {
            Debug.Assert(firstPoint != null);
            var fp = firstPoint.Value;
            var p1 = new Float3(
                Math.Min(fp.X, secondPoint.X),
                Math.Min(fp.Y, secondPoint.Y),
                Math.Min(fp.Z, secondPoint.Z)
            );
            var p2 = new Float3(
                Math.Max(fp.X, secondPoint.X),
                Math.Max(fp.Y, secondPoint.Y),
                Math.Max(fp.Z, secondPoint.Z)
            );
            Start = p1;
            End = p2;
        }

        public override void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            models.Overlay.Overlays.Remove(this);
            base.Dispose();
        }
    }
}
