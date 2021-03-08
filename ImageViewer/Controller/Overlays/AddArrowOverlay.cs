using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Controller.Overlays
{
    public class AddArrowOverlay : ArrowOverlay, IDisplayOverlay
    {
        private readonly ModelsEx models;
        private bool isDisposed = false;
        private Float2? firstPoint;

        public AddArrowOverlay(ModelsEx models) : base(models)
        {
            this.models = models;
            models.Overlay.Overlays.Add(this);
            models.Display.UserInfo = "Click left to start arrow";

            View = null; // TODO toolbar?
        }

        public void MouseMove(Size3 texel)
        {
            if (firstPoint == null) return; // nothing to draw

            Arrows.Clear();
            Arrows.Add(new Arrow
            {
                Start = firstPoint.Value,
                End = GetCurrentCoords(texel),
                Width = models.Settings.ArrowWidth,
                Color = models.Settings.ArrowColor
            });
        }

        private Float2 GetCurrentCoords(Size3 texel)
        {
            return texel.ToCoords(models.Images.GetSize(models.Display.ActiveMipmap)).XY;
        }

        public void MouseClick(MouseButton button, bool down, Size3 texel)
        {
            if (down) return;
            if (button != MouseButton.Left) return;

            if (firstPoint == null)
            {
                // set start point
                firstPoint = GetCurrentCoords(texel);
                // MouseMove(texel); // force draw
            }
            else
            {
                // finish arrow
                var dia = new ArrowDialog(models.Settings.ArrowColor, models.Settings.ArrowWidth);
                if (models.Window.ShowDialog(dia) == true)
                {
                    models.Settings.ArrowColor = dia.Color;
                    models.Settings.ArrowWidth = dia.StrokeWidth;

                    Debug.Assert(firstPoint.HasValue);

                    var a = new ArrowOverlay.Arrow
                    {
                        Start = firstPoint.Value,
                        End = GetCurrentCoords(texel),
                        Color = dia.Color,
                        Width = dia.StrokeWidth
                    };
                    models.Arrows.Arrows.Add(a);
                }

                models.Display.ActiveOverlay = null;
            }

        }

        public bool OnKeyDown(Key key)
        {
            return false;
        }

        public UIElement View { get; }

        public override void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            models.Overlay.Overlays.Remove(this);
            base.Dispose();
            models.Display.UserInfo = "";
        }
    }
}
