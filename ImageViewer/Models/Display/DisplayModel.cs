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
using ImageFramework.DirectX;
using ImageFramework.DirectX.Query;
using ImageFramework.Model;
using ImageFramework.Utility;
using SharpDX;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ImageViewer.Models.Display
{
    public interface IExtendedDisplayModel : IDisposable, INotifyPropertyChanged { }
    public class DisplayModel : INotifyPropertyChanged, IDisposable
    {
        public enum ViewMode
        {
            Empty,
            Single,
            CubeMap,
            Polar,
            CubeCrossView,
            RayCasting,
            ShearWarp
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
                OnPropertyChanged(nameof(AvailableViews));

                // active view must be within available views
                Debug.Assert(availableViews.Count != 0);
                ActiveView = availableViews[0];
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
                extendedView?.Dispose();
                if (activeView == ViewMode.Single && models.Images.ImageType == typeof(Texture3D))
                {
                    extendedView = new Single3DDisplayModel(models, this);
                }
                else if (ActiveView == ViewMode.RayCasting)
                {
                    extendedView = new RayCastingDisplayModel(models, this);
                }
                else
                {
                    extendedView = null;
                }
                OnPropertyChanged(nameof(ActiveView));
                OnPropertyChanged(nameof(ExtendedViewData));
            }
        }

        private IDisplayOverlay activeOverlay = null;

        public IDisplayOverlay ActiveOverlay
        {
            get => activeOverlay;
            set
            {
                if(ReferenceEquals(value, activeOverlay)) return;
                activeOverlay?.Dispose();
                activeOverlay = value;
                activeOverlay?.MouseMove(TexelPosition);
                OnPropertyChanged(nameof(ActiveOverlay));
            }
        }

        private IExtendedDisplayModel extendedView = null;
        public IExtendedDisplayModel ExtendedViewData => extendedView;

        private static readonly float[] ZOOM_POINTS = { 0.01f, 0.02f, 0.05f, 0.1f, 0.25f, 0.33f, 0.5f, 0.75f, 1.0f, 1.25f, 1.5f, 2.0f, 4.0f, 8.0f, 16.0f, 32.0f, 64.0f, 128.0f };
        private float zoom = 1.0f;
        public float Zoom
        {
            get => zoom;
            set
            {
                var clamped = Math.Min(Math.Max(value, 0.01f), 128.0f);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (clamped == zoom) return;
                zoom = clamped;
                OnPropertyChanged(nameof(Zoom));
            }
        }
        public void IncreaseZoom()
        {
            // Get the first number larger than zoom
            foreach (float zoomPoint in ZOOM_POINTS)
            {
                if (zoomPoint > zoom)
                {
                    zoom = zoomPoint;
                    OnPropertyChanged(nameof(Zoom));
                    break;
                }
            }
        }
        public void DecreaseZoom()
        {
            // Get the first number smaller than zoom
            foreach (float zoomPoint in ZOOM_POINTS.Reverse())
            {
                if (zoomPoint < zoom)
                {
                    zoom = zoomPoint;
                    OnPropertyChanged(nameof(Zoom));
                    break;
                }
            }
        }

        private AdvancedGpuTimer.Stats frameTime = AdvancedGpuTimer.Stats.Zero;

        public AdvancedGpuTimer.Stats FrameTime
        {
            get => frameTime;
            set
            {
                frameTime = value;
                OnPropertyChanged(nameof(FrameTime));
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

        private bool isExporting = false;

        /// <summary>
        /// indicates if the exporting dialog is open
        /// </summary>
        public bool IsExporting
        {
            get => isExporting;
            set
            {
                if (value == isExporting) return;
                isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
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
                Debug.Assert(value == 0 || value >= 0 && value < models.Images.NumLayers);
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
                Debug.Assert(value == 0 || value >= 0 && value < models.Images.NumMipmaps);
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

        private bool displayNegative = true;

        public bool DisplayNegative
        {
            get => displayNegative;
            set
            {
                if (value == displayNegative) return;
                displayNegative = value;
                OnPropertyChanged(nameof(DisplayNegative));
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

        private string userInfo = "";

        public string UserInfo
        {
            get => userInfo;
            set
            {
                if(userInfo == value) return;
                userInfo = value;
                OnPropertyChanged(nameof(UserInfo));
            }
        }

        // previous mouse position on the texture (should be used for context menu things because the texel)
        public Size3 PrevTexelPosition { get; private set; } = Size3.Zero;

        private Size3 texelPosition = Size3.Zero;
        // the mouse position on the texture
        public Size3 TexelPosition
        {
            get => texelPosition;
            set
            {
                PrevTexelPosition = texelPosition;
                if (value.Equals(texelPosition)) return;
                texelPosition = value;
                OnPropertyChanged(nameof(TexelPosition));

                ActiveOverlay?.MouseMove(texelPosition);
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
                        if (models.Images.Images[0].Image.HasCubemap)
                        {
                            // cube map should be the default view
                            modes.Insert(0, ViewMode.CubeMap);
                            modes.Insert(1, ViewMode.CubeCrossView);
                        }
                        else if (models.Images.NumLayers == 1 && models.Images.ImageType == typeof(TextureArray2D))
                        {
                            modes.Add(ViewMode.Polar);
                        }
                        else if(models.Images.ImageType == typeof(Texture3D))
                        {
                            modes.Insert(0, ViewMode.RayCasting);
                            modes.Insert(1, ViewMode.ShearWarp);
                        }

                        AvailableViews = modes;

                        // initial aspect ratio calculation for that image
                        RecomputeAspectRatio(lastClientSize);
                    }
                    break;
                case nameof(ImagesModel.Size):
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
                    models.Images.GetWidth(0) / (float)clientSize.Width,
                    models.Images.GetHeight(0) / (float)clientSize.Height,
                    1.0f);
            }

            ClientAspectRatio = Matrix.Scaling(
                clientSize.Width / (float)clientSize.Height, 1.0f, 1.0f
            );
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            activeOverlay?.Dispose();
            extendedView?.Dispose();
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
                case DisplayModel.ViewMode.RayCasting:
                case DisplayModel.ViewMode.ShearWarp:
                case DisplayModel.ViewMode.Single:
                case DisplayModel.ViewMode.Empty:
                    return false;
                default:
                    return true;
            }
        }
    }
}