using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Annotations;
using ImageFramework.Model;
using SharpDX;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ImageViewer.Models
{
    public class DisplayModel : INotifyPropertyChanged
    {
        public enum ViewMode
        {
            Empty,
            Single,
            CubeMap,
            Polar,
            CubeCrossView
        }

        public enum SplitMode
        {
            Vertical,
            Horizontal
        }

        private readonly ImageFramework.Model.Models models;

        private List<ViewMode> availableViews = new List<ViewMode>() { ViewMode.Empty };
        public List<ViewMode> AvailableViews
        {
            get => availableViews;
            private set
            {
                availableViews = value;
                // active view must be within available views
                Debug.Assert(availableViews.Count != 0);
                activeView = availableViews[0];

                OnPropertyChanged(nameof(AvailableViews));
                OnPropertyChanged(nameof(ActiveView));
            }
        }

        private ViewMode activeView = ViewMode.Empty;
        public ViewMode ActiveView
        {
            get => activeView;
            set
            {
                if (value == activeView) return;
                // active view must be in available views
                Debug.Assert(availableViews.Contains(value));
                activeView = value;
                OnPropertyChanged(nameof(ActiveView));
            }
        }

        private float zoom = 1.0f;
        public float Zoom
        {
            get => zoom;
            set
            {
                var clamped = Math.Min(Math.Max(value, 0.01f), 100.0f);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (clamped == zoom) return;
                zoom = clamped;
                OnPropertyChanged(nameof(Zoom));
            }
        }


        // for values >= 0 => multiplier = pow(2, multiplierExponent)
        // for values < 0 => multiplier = pow(2, 1 / abs(multiplierExponent))
        private int multiplierExponent = 0;
        private static int maxMultiplier = 60;

        public void IncreaseMultiplier()
        {
            if (multiplierExponent >= maxMultiplier) return;

            ++multiplierExponent;
            OnPropertyChanged(nameof(Multiplier));
        }

        public void DecreaseMultiplier()
        {
            if (multiplierExponent <= -maxMultiplier) return;

            --multiplierExponent;
            OnPropertyChanged(nameof(Multiplier));
        }

        public float Multiplier
        {
            get
            {
                if (multiplierExponent >= 0)
                    return (float)Math.Pow(2.0, multiplierExponent);
                return (float)(1.0 / Math.Pow(2.0f, -multiplierExponent));
            }
        }

        public string MultiplierString
        {
            get
            {
                var num = (long)1 << Math.Abs(multiplierExponent);
                var str = num.ToString();
                if (multiplierExponent < 0)
                    return "1/" + str;
                return str;
            }
        }

        private float aperture = (float)Math.PI / 2.0f;
        public float Aperture
        {
            get => aperture;
            set
            {
                var clamped = Math.Min(Math.Max(value, 0.01f), (float)Math.PI * 0.90f);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (clamped == aperture) return;
                aperture = clamped;
                OnPropertyChanged(nameof(Aperture));
            }
        }

        private Matrix imageAspectRatio = Matrix.Identity;
        public Matrix ImageAspectRatio
        {
            get => imageAspectRatio;
            set
            {
                if (imageAspectRatio.Equals(value)) return;
                imageAspectRatio = value;
                OnPropertyChanged(nameof(ImageAspectRatio));
            }
        }

        private Matrix clientAspectRatio = Matrix.Identity;

        public Matrix ClientAspectRatio
        {
            get => clientAspectRatio;
            set
            {
                if (clientAspectRatio.Equals(value)) return;
                clientAspectRatio = value;
                OnPropertyChanged(nameof(ClientAspectRatio));
            }
        }

        private int activeLayer = 0;
        public int ActiveLayer
        {
            get => activeLayer;
            set
            {
                Debug.Assert(value == 0 || (value >= 0 && value < models.Images.NumLayers));
                if (value == activeLayer) return;
                activeLayer = value;
                OnPropertyChanged(nameof(ActiveLayer));
            }
        }

        private int activeMipmap = 0;
        public int ActiveMipmap
        {
            get => activeMipmap;
            set
            {
                Debug.Assert(value == 0 || (value >= 0 && value < models.Images.NumMipmaps));
                if (value == activeMipmap) return;
                activeMipmap = value;
                OnPropertyChanged(nameof(ActiveMipmap));
            }
        }

        private bool linearInterpolation = false;
        public bool LinearInterpolation
        {
            get => linearInterpolation;
            set
            {
                if (value == linearInterpolation) return;
                linearInterpolation = value;
                OnPropertyChanged(nameof(LinearInterpolation));
            }
        }

        private bool showCropRectangle = true;
        public bool ShowCropRectangle
        {
            get => showCropRectangle;
            set
            {
                if (showCropRectangle == value) return;
                showCropRectangle = value;
                OnPropertyChanged(nameof(ShowCropRectangle));
            }
        }

        private SplitMode splitMode = SplitMode.Vertical;
        public SplitMode Split
        {
            get => splitMode;
            set
            {
                if (value == splitMode) return;
                splitMode = value;
                OnPropertyChanged(nameof(Split));
            }
        }

        private Point texelPosition = new Point(0, 0);
        // the mouse position on the texture
        public Point TexelPosition
        {
            get => texelPosition;
            set
            {
                if (value == null || value.Equals(texelPosition)) return;
                texelPosition = value;
                OnPropertyChanged(nameof(TexelPosition));
            }
        }

        public int MinTexelRadius { get; } = 0;
        public int MaxTexelRadius { get; } = 10;

        private int texelRadius = 0;
        public int TexelRadius
        {
            get => texelRadius;
            set
            {
                var clamped = Math.Min(Math.Max(value, MinTexelRadius), MaxTexelRadius);
                if (texelRadius == clamped) return;
                texelRadius = clamped;
                OnPropertyChanged(nameof(TexelRadius));
            }
        }

        public DisplayModel(ImageFramework.Model.Models models)
        {
            this.models = models;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    // was the image resettet?
                    if (models.Images.NumImages == 0)
                    {
                        ActiveMipmap = 0;
                        ActiveLayer = 0;
                        // this will reset active view as well
                        AvailableViews = new List<ViewMode> { ViewMode.Empty };
                    }
                    else if (models.Images.PrevNumImages == 0)
                    {
                        // first image was added
                        var modes = new List<ViewMode> { ViewMode.Single };
                        if (models.Images.NumLayers == 6)
                        {
                            // cube map should be the default view
                            modes.Insert(0, ViewMode.CubeMap);
                            modes.Insert(1, ViewMode.CubeCrossView);
                        }
                        else if (models.Images.NumLayers == 1)
                        {
                            modes.Add(ViewMode.Polar);
                        }

                        AvailableViews = modes;

                        // initial aspect ratio calculation for that image
                        RecomputeAspectRatio(lastClientSize);
                    }
                    break;
                case nameof(ImagesModel.Width):
                case nameof(ImagesModel.Height):
                    RecomputeAspectRatio(lastClientSize);
                    break;
            }
        }

        private Size lastClientSize = Size.Empty;
        public void RecomputeAspectRatio(Size clientSize)
        {
            lastClientSize = clientSize;

            if (models.Images.NumImages > 0) 
            {
                ImageAspectRatio = Matrix.Scaling(
                    (float)models.Images.GetWidth(0) / (float)clientSize.Width,
                    (float)models.Images.GetHeight(0) / (float)clientSize.Height,
                    1.0f);
            }

            ClientAspectRatio = Matrix.Scaling(
                (float)clientSize.Width / (float)clientSize.Height, 1.0f, 1.0f
            );
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// extension methods for view mode enumeration
    /// </summary>
    public static class ViewModeExtension
    {
        public static bool IsDegree(this DisplayModel.ViewMode vm)
        {
            switch (vm)
            {
                case DisplayModel.ViewMode.CubeCrossView:
                case DisplayModel.ViewMode.Single:
                case DisplayModel.ViewMode.Empty:
                    return false;
                default:
                    return true;
            }
        }
    }
}