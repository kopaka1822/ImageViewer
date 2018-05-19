using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TextureViewer.Annotations;

namespace TextureViewer.Models
{
    public class DisplayModel : INotifyPropertyChanged
    {
        public enum GrayscaleMode
        {
            Disabled,
            Red,
            Green,
            Blue,
            Alpha
        }

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

        private readonly ImagesModel imagesModel;
        private readonly OpenGlContext glContext;

        /// <summary>
        /// indicates if the grascale mode was set by the application for the first image
        /// </summary>
        private bool autoEnabledGrayscale = false;

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

        private List<ViewMode> availableViews = new List<ViewMode>(){ViewMode.Empty};
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

        private Matrix4 aspectRatio = Matrix4.Identity;
        public Matrix4 AspectRatio
        {
            get => aspectRatio;
            set
            {
                if (aspectRatio.Equals(value)) return;
                aspectRatio = value;
                OnPropertyChanged(nameof(AspectRatio));
            }
        }

        private int activeLayer = 0;
        public int ActiveLayer
        {
            get => activeLayer;
            set
            {
                Debug.Assert(value == 0 || (value >= 0 && value < imagesModel.NumLayers));
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
                Debug.Assert(value == 0 || (value >= 0 && value < imagesModel.NumMipmaps));
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

        private GrayscaleMode grayscale = GrayscaleMode.Disabled;
        public GrayscaleMode Grayscale
        {
            get => grayscale;
            set
            {
                if (value == grayscale) return;
                grayscale = value;
                OnPropertyChanged(nameof(Grayscale));
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

        public DisplayModel(ImagesModel imagesModel, OpenGlContext glContext)
        {
            this.imagesModel = imagesModel;
            this.glContext = glContext;
            this.imagesModel.PropertyChanged += ImagesModelOnPropertyChanged;
            this.glContext.PropertyChanged += GlContextOnPropertyChanged;
        }

        private void GlContextOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(OpenGlContext.ClientSize):
                    RecomputeAspectRatio();
                    break;
            }
        }

        private void ImagesModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    // was the image resettet?
                    if (imagesModel.NumImages == 0)
                    {
                        ActiveMipmap = 0;
                        ActiveLayer = 0;
                        // this will reset active view as well
                        AvailableViews = new List<ViewMode> { ViewMode.Empty };
                    }
                    else if (imagesModel.PrevNumImages == 0)
                    {
                        // first image was added
                        var modes = new List<ViewMode> { ViewMode.Single };
                        if (imagesModel.NumLayers == 6)
                        {
                            // cube map should be the default view
                            modes.Insert(0, ViewMode.CubeMap);
                            modes.Insert(1, ViewMode.CubeCrossView);
                        }
                        else if (imagesModel.NumLayers == 1)
                        {
                            modes.Add(ViewMode.Polar);
                        }

                        AvailableViews = modes;

                        // enable grayscale?
                        if (imagesModel.IsGrayscale)
                        {
                            autoEnabledGrayscale = true;
                            Grayscale = GrayscaleMode.Red;
                        }
                        else
                        {
                            autoEnabledGrayscale = false;
                        }

                        // initial aspect ratio calculation for that image
                        RecomputeAspectRatio();
                    }
                    else // more images were added to the existing ones
                    {
                        if (!imagesModel.IsGrayscale && Grayscale == GrayscaleMode.Red && autoEnabledGrayscale)
                        {
                            // disable grayscale since not all images are grayscale anymore
                            Grayscale = GrayscaleMode.Disabled;
                            // forget this setting
                            autoEnabledGrayscale = false;
                        }
                    }
                    break;
            }
        }

        private void RecomputeAspectRatio()
        {
            if (imagesModel.NumImages > 0)
            {
                AspectRatio = Matrix4.CreateScale(
                    (float)imagesModel.GetWidth(0) / (float)glContext.ClientSize.Width,
                    (float)imagesModel.GetHeight(0) / (float)glContext.ClientSize.Height,
                    1.0f);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
