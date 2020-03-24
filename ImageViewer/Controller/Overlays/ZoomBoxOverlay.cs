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
    public class ZoomBoxOverlay : CropOverlay, IDisplayOverlay, INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private bool isDisposed = false;
        private Float3? firstPoint;
        private static float? prevRatio = null;

        public ZoomBoxOverlay(ModelsEx models) : base(models)
        {
            this.models = models;
            IsEnabled = true;
            Layer = models.Display.ActiveLayer;
            models.Overlay.Overlays.Add(this);
            models.Display.UserInfo = "Click left to start zoombox";

            if (prevRatio.HasValue)
            {
                curRatio = prevRatio.Value;
                keepRatio = true;
            }

            ToggleRatioCommand = new ActionCommand(() => KeepRatio = !KeepRatio);
            View = new ZoomBoxToolbar
            {
                DataContext = this
            };
        }

        private Size3 CurDim => models.Images.Size.GetMip(models.Display.ActiveMipmap);

        // view model bindings
        public bool EnableBoxes => firstPoint != null;

        private float curRatio = 1.0f;

        public float? LastRatio => keepRatio ? (float?)curRatio : null;

        private bool keepRatio = false;

        public bool KeepRatio
        {
            get => keepRatio;
            set
            {
                if (!Start.HasValue || !End.HasValue) return;

                keepRatio = value;
                UpdateRatio();
                OnPropertyChanged(nameof(KeepRatio));
            }
        }

        private void UpdateRatio()
        {
            if (!Start.HasValue || !End.HasValue) return;
            if (!keepRatio) return;

            curRatio = (End.Value.X - Start.Value.X) / (End.Value.Y - Start.Value.Y);
        }

        public ICommand ToggleRatioCommand { get; }

        public int BoxStartX
        {
            get => firstPoint?.ToPixels(CurDim).X ?? 0;
            set
            {
                if (!firstPoint.HasValue || !Start.HasValue || !End.HasValue) return;
                // back to float coordinates
                float fx = Utility.Clamp((value + 0.5f) / CurDim.X, 0.0f, 1.0f);

                // adjust cropping rectangle
                if (firstPoint.Value.X == Start.Value.X)
                {
                    Start = new Float3(fx, Start.Value.YZ);
                }
                else // first Point is end
                {
                    End = new Float3(fx, End.Value.YZ);
                }

                // points invalid?
                if (Start.Value.X > End.Value.X)
                {
                    var sx = Start.Value.X;
                    Start = new Float3(End.Value.X, Start.Value.YZ);
                    End = new Float3(sx, End.Value.YZ);
                }

                firstPoint = new Float3(fx, firstPoint.Value.YZ);

                OnPropertyChanged(nameof(BoxStartX));
                OnPropertyChanged(nameof(BoxWidth));
            }
        }

        public int BoxStartY
        {
            get => firstPoint?.ToPixels(CurDim).Y ?? 0;
            set
            {
                if (!firstPoint.HasValue || !Start.HasValue || !End.HasValue) return;
                // back to float coordinates
                float fy = Utility.Clamp((value + 0.5f) / CurDim.Y, 0.0f, 1.0f);

                // adjust cropping rectangle
                if (firstPoint.Value.Y == Start.Value.Y)
                {
                    Start = new Float3(Start.Value.X, fy, Start.Value.Z);
                }
                else // first Point is end
                {
                    End = new Float3(End.Value.X, fy, End.Value.Z);
                }

                // points invalid?
                if (Start.Value.Y > End.Value.Y)
                {
                    var sy = Start.Value.Y;
                    Start = new Float3(Start.Value.X, End.Value.Y, Start.Value.Z);
                    End = new Float3(End.Value.X, sy, End.Value.Z);
                }

                firstPoint = new Float3(firstPoint.Value.X, fy, firstPoint.Value.Y);

                OnPropertyChanged(nameof(BoxStartY));
                OnPropertyChanged(nameof(BoxHeight));
            }
        }

        public int BoxWidth
        {
            get
            {
                if (!End.HasValue || !Start.HasValue) return 0;
                var dim = CurDim;
                return End.Value.ToPixels(dim).X - Start.Value.ToPixels(dim).X + 1;
            }
            set
            {
                if (!firstPoint.HasValue || !Start.HasValue || !End.HasValue) return;

                // very important to remember correct first point
                float min = 1.0f / (models.Images.Size.X * 128.0f);
                // back to float coordinates
                float w = Utility.Clamp((float)Math.Max(value - 1, 0) / CurDim.X, min, 1.0f);

                if (firstPoint.Value.X == Start.Value.X)
                {
                    End = new Float3(Math.Min(firstPoint.Value.X + w, 1.0f), End.Value.YZ);
                }
                else
                {
                    Start = new Float3(Math.Max(firstPoint.Value.X - w, 0.0f), Start.Value.YZ);
                }

                OnPropertyChanged(nameof(BoxWidth));

                UpdateRatio();
            }
        }

        public int BoxHeight
        {
            get
            {
                if (!End.HasValue || !Start.HasValue) return 0;
                var dim = CurDim;
                return End.Value.ToPixels(dim).Y - Start.Value.ToPixels(dim).Y + 1;
            }
            set
            {
                if (!firstPoint.HasValue || !Start.HasValue || !End.HasValue) return;

                // very important to remember correct first point
                float min = 1.0f / (models.Images.Size.Y * 128.0f);
                // back to float coordinates
                float h = Utility.Clamp((float)Math.Max(value - 1, 0) / CurDim.Y, min, 1.0f);

                if (firstPoint.Value.Y == Start.Value.Y)
                {
                    End = new Float3(End.Value.X, Math.Min(firstPoint.Value.Y + h, 1.0f), End.Value.Z);
                }
                else
                {
                    Start = new Float3(Start.Value.X, Math.Max(firstPoint.Value.Y - h, 0.0f), Start.Value.Z);
                }

                OnPropertyChanged(nameof(BoxHeight));

                UpdateRatio();
            }
        }

        public void MouseMove(Size3 texel)
        {
            Layer = models.Display.ActiveLayer;
            if (firstPoint == null) return;
            SetCropRect(GetCurrentCoords(texel));
            var dim = CurDim;
            var start = Start.Value.ToPixels(dim);
            var end = End.Value.ToPixels(dim);
            OnPropertyChanged(nameof(BoxWidth));
            OnPropertyChanged(nameof(BoxHeight));
            models.Display.UserInfo =
                $"Zoombox size: {end.X - start.X + 1} x {end.Y - start.Y + 1}";
        }

        public void MouseClick(MouseButton button, bool down, Size3 texel)
        {
            if(down) return;
            if (button != MouseButton.Left) return;
            if (firstPoint == null)
            {
                firstPoint = GetCurrentCoords(texel);
                SetCropRect(GetCurrentCoords(texel));
                OnPropertyChanged(nameof(EnableBoxes));
                OnPropertyChanged(nameof(BoxStartX));
                OnPropertyChanged(nameof(BoxStartY));
            }
            else
            {
                SetCropRect(GetCurrentCoords(texel));
                var dia = new ZoomBoxDialog(models.Settings.ZoomBoxColor, models.Settings.ZoomBoxBorder);
                if (models.Window.ShowDialog(dia) == true)
                {
                    models.Settings.ZoomBoxColor = dia.Color;
                    models.Settings.ZoomBoxBorder = dia.BorderSize;

                    Debug.Assert(Start.HasValue);
                    Debug.Assert(End.HasValue);

                    // add zoom box
                    var box = new BoxOverlay.Box
                    {
                        Border = dia.BorderSize,
                        Color = dia.Color,
                        Start = Start.Value.XY,
                        End = End.Value.XY,
                    };
                    models.ZoomBox.Boxes.Add(box);
                }
                models.Display.ActiveOverlay = null;
            }
        }

        public bool OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.Q: // snap ratio to quad
                    KeepRatio = true;
                    var dim = Math.Max(BoxWidth, BoxHeight);
                    BoxWidth = dim;
                    BoxHeight = dim;
                    return true;
                case Key.R:
                    KeepRatio = !KeepRatio;
                    return true;
                case Key.Escape:
                    models.Display.ActiveOverlay = null;
                    return true;
            }

            return false;
        }

        public UIElement View { get; }

        private Float3 GetCurrentCoords(Size3 texel)
        {
            return texel.ToCoords(models.Images.GetSize(models.Display.ActiveMipmap));
        }

        // sign function that only returns 1 or -1
        private float Sign(float x)
        {
            return x < 0.0f ? -1.0f : 1.0f;
        }

        private void SetCropRect(Float3 secondPoint)
        {
            Debug.Assert(firstPoint != null);
            var fp = firstPoint.Value;
            if (keepRatio)
            {
                float sdx = secondPoint.X - fp.X;
                float sdy = secondPoint.Y - fp.Y;
                float dx = Math.Abs(sdx);
                float dy = Math.Abs(sdy);
                float rt = dx / dy;
                if (dx != 0.0f || dy != 0.0f)
                {
                    if (rt > curRatio)
                    {
                        secondPoint.Y = fp.Y + Sign(sdy) * dx / curRatio;
                    }
                    else if (rt < curRatio)
                    {
                        secondPoint.X = fp.X + Sign(sdx) * dy * curRatio;
                    }

                    var newRt = Math.Abs((fp.X - secondPoint.X) / (fp.Y - secondPoint.Y));
                    Debug.Assert(Math.Abs(newRt - curRatio) < 0.0001f);
                }
            }
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
            models.Display.UserInfo = "";
            prevRatio = LastRatio;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
