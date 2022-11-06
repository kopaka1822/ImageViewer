using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Views.Dialog;
using ImageViewer.Views.Display;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.Overlays
{
    public class ZoomBoxOverlay : GenericBoxOverlay
    {
        public ZoomBoxOverlay(ModelsEx models) : base(models)
        {
        }

        protected override void OnFinished(Float2 start, Float2 end)
        {
            var dia = new ZoomBoxDialog(models.Settings.ZoomBoxColor, models.Settings.ZoomBoxBorder);
            if (models.Window.ShowDialog(dia) == true)
            {
                models.Settings.ZoomBoxColor = dia.Color;
                models.Settings.ZoomBoxBorder = dia.BorderSize;

                // add zoom box
                var box = new BoxOverlay.Box
                {
                    Border = dia.BorderSize,
                    Color = dia.Color,
                    Start = start,
                    End = end,
                };
                models.ZoomBox.Boxes.Add(box);
            }
        }
    }
}
