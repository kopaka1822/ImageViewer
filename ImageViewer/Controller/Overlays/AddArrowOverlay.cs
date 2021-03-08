using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Views.Dialog;
using ImageViewer.Views.Display;

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

            View = new ArrowsToolbar
            {
                DataContext = this
            };
        }

        public int StrokeWidth
        {
            get => models.Settings.ArrowWidth;
            set
            {
                models.Settings.ArrowWidth = value;
                MouseMove(models.Display.TexelPosition ?? Size3.Zero);
            }
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
                models.Display.UserInfo = "Click left to finish arrow";
            }
            else
            {
                // finish arrow
                var dia = new ArrowDialog(models.Settings.ArrowColor);
                if (models.Window.ShowDialog(dia) == true)
                {
                    models.Settings.ArrowColor = dia.Color;

                    Debug.Assert(firstPoint.HasValue);

                    var a = new ArrowOverlay.Arrow
                    {
                        Start = firstPoint.Value,
                        End = GetCurrentCoords(texel),
                        Color = dia.Color,
                        Width = StrokeWidth
                    };
                    models.Arrows.Arrows.Add(a);
                }

                models.Display.ActiveOverlay = null;
            }

        }

        public bool OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Add:
                case Key.OemPlus:
                    StrokeWidth += 1;
                    return true;
                case Key.Subtract:
                case Key.OemMinus:
                    StrokeWidth = Math.Max(1, StrokeWidth - 1);
                    return true;
                case Key.Escape:
                    models.Display.ActiveOverlay = null;
                    return true;
            }

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
