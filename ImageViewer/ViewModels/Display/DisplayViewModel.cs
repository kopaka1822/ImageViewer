using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Controller.Overlays;
using ImageViewer.Controller.TextureViews;
using ImageViewer.Controller.TextureViews.Texture3D;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Views;
using Color = System.Windows.Media.Color;
using RayCastingView = ImageViewer.Views.Display.RayCastingView;
using Single3DView = ImageViewer.Views.Display.Single3DView;

namespace ImageViewer.ViewModels.Display
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private static readonly ListItemViewModel<int> EmptyMipMap = new ListItemViewModel<int>
        {
            Name = "No Mipmap",
            Cargo = -1
        };
        private static readonly ListItemViewModel<int> EmptyLayer = new ListItemViewModel<int>
        {
            Name = "No Layer",
            Cargo = -1
        };
        private static readonly ListItemViewModel<DisplayModel.ViewMode> EmptyViewMode = new ListItemViewModel<DisplayModel.ViewMode>
        {
            Name = "Empty",
            Cargo = DisplayModel.ViewMode.Empty
        };

        public DisplayViewModel(ModelsEx models)
        {
            this.models = models;
            selectedSplitMode = AvailableSplitModes[models.Display.Split == DisplayModel.SplitMode.Vertical ? 0 : 1];
            models.PropertyChanged += ModelsOnPropertyChanged;
            models.Display.PropertyChanged += DisplayOnPropertyChanged;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
            models.Settings.PropertyChanged += SettingsOnPropertyChanged;

            CreateViewModes();
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.FlipYAxis):
                    OnPropertyChanged(nameof(TexelPosition));
                    OnPropertyChanged(nameof(FlipYAxis));
                    OnPropertyChanged(nameof(DxSystemVisibility));
                    OnPropertyChanged(nameof(GlSystemVisibility));
                    break;
                case nameof(SettingsModel.AlphaBackground):
                    OnPropertyChanged(nameof(IsAlphaBlack));
                    OnPropertyChanged(nameof(IsAlphaWhite));
                    OnPropertyChanged(nameof(IsAlphaCheckers));
                    OnPropertyChanged(nameof(IsAlphaTheme));
                    break;
                case nameof(SettingsModel.NaNColor):
                    OnPropertyChanged(nameof(NaNColor));
                    break;
            }
        }

        public SolidColorBrush NaNColor
        {
            get
            {
                var c = models.Settings.NaNColor;
                return new SolidColorBrush(Color.FromScRgb(1.0f, c.Red, c.Green, c.Blue));
            }
        }

        public string UserInfo => models.Display.UserInfo;

        public bool HasKeyToInvoke(Key key)
        {
            switch (key)
            {
                case Key.Add:
                case Key.OemPlus:
                case Key.Subtract:
                case Key.OemMinus:
                    return true;
            }

            return false;
        }

        public void InvokeKey(Key key)
        {
            switch (key)
            {
                case Key.Add:
                case Key.OemPlus:
                    models.Display.IncreaseMultiplier();
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    models.Display.DecreaseMultiplier();
                    break;
            }
        }

        public bool HasPriorityKeyInvoked(Key key)
        {
            if (models.Display.ActiveOverlay == null) return false;
            return models.Display.ActiveOverlay.OnKeyDown(key);
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumMipmaps):
                case nameof(ImagesModel.Size):
                    CreateMipMapList();
                    break;
                case nameof(ImagesModel.NumLayers):
                    CreateLayersList();
                    OnPropertyChanged(nameof(AllowMovieOverlay));
                    if (models.Images.NumLayers > 1 && models.Display.ActiveOverlay == null)
                    {
                        ShowMovieOverlay = true;
                    }
                    break;
            }
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DisplayModel.LinearInterpolation):
                    OnPropertyChanged(nameof(LinearInterpolation));
                    break;

                case nameof(DisplayModel.DisplayNegative):
                    OnPropertyChanged(nameof(DisplayNegative));
                    break;

                case nameof(DisplayModel.ShowCropRectangle):
                    OnPropertyChanged(nameof(ShowCropRectangle));
                    break;

                case nameof(DisplayModel.ActiveLayer):
                    SelectedLayer = AvailableLayers.Count != 0
                        ? AvailableLayers[models.Display.ActiveLayer]
                        : EmptyLayer;
                    break;

                case nameof(DisplayModel.ActiveMipmap):
                    SelectedMipMap = AvailableMipMaps.Count != 0
                        ? AvailableMipMaps[models.Display.ActiveMipmap]
                        : EmptyMipMap;
                    break;

                case nameof(DisplayModel.AvailableViews):
                    CreateViewModes();
                    break;

                case nameof(DisplayModel.ActiveView):
                    var selected = EmptyViewMode;
                    foreach (var item in AvailableViewModes)
                    {
                        if (item.Cargo == models.Display.ActiveView)
                            selected = item;
                    }

                    SelectedViewMode = selected;

                    OnPropertyChanged(nameof(ChooseLayers));
                    OnPropertyChanged(nameof(Zoom));
                    break;

                case nameof(DisplayModel.Split):
                    SelectedSplitMode =
                        AvailableSplitModes[models.Display.Split == DisplayModel.SplitMode.Vertical ? 0 : 1];
                    break;

                case nameof(DisplayModel.TexelPosition):
                    OnPropertyChanged(nameof(TexelPosition));
                    break;

                case nameof(DisplayModel.Aperture):
                case nameof(DisplayModel.Zoom):
                    OnPropertyChanged(nameof(Zoom));
                    break;

                case nameof(DisplayModel.Multiplier):
                    OnPropertyChanged(nameof(Multiplier));
                    break;

                case nameof(DisplayModel.ExtendedViewData):
                    extendedView?.Dispose();
                    extendedView = null;

                    if (models.Display.ExtendedViewData != null)
                    {
                        if (models.Display.ExtendedViewData is Single3DDisplayModel)
                        {
                            var view = new Single3DView(models);
                            extendedView = view; 
                            models.Window.Window.ImagesTab.ExtendedViewHost.Child = view;
                        }
                        else if (models.Display.ExtendedViewData is RayCastingDisplayModel)
                        {
                            var view = new RayCastingView(models);
                            extendedView = view;
                            models.Window.Window.ImagesTab.ExtendedViewHost.Child = view;
                        }
                        else Debug.Assert(false);
                    }
                    else
                    {
                        models.Window.Window.ImagesTab.ExtendedViewHost.Child = null;
                    }

                    OnPropertyChanged(nameof(ExtendedViewVisibility));
                    break;

                case nameof(DisplayModel.FrameTime):
                    OnPropertyChanged(nameof(FrameTime));
                    break;

                case nameof(DisplayModel.UserInfo):
                    OnPropertyChanged(nameof(UserInfo));
                    break;

                case nameof(DisplayModel.ActiveOverlay):
                    var host = models.Window.Window.OverlayViewHost;
                    if (models.Display.ActiveOverlay?.View == null)
                    {
                        // remove overlay view
                        host.Visibility = Visibility.Collapsed;
                        host.Child = null;
                    }
                    else // set overlay view
                    {
                        host.Child = models.Display.ActiveOverlay.View;
                        host.Visibility = Visibility.Visible;
                    }
                    // this has potentially changed
                    OnPropertyChanged(nameof(ShowMovieOverlay));
                    break;
            }
        }

        private void ModelsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImageFramework.Model.Models.NumEnabled):
                    OnPropertyChanged(nameof(EnableSplitMode));
                    break;
            }
        }

        public string FrameTime => models.Display.FrameTime.Last.ToString("f2", ImageFramework.Model.Models.Culture) + " ms";

        public bool LinearInterpolation
        {
            get => models.Display.LinearInterpolation;
            set => models.Display.LinearInterpolation = value;
        }

        public bool DisplayNegative
        {
            get => models.Display.DisplayNegative;
            set => models.Display.DisplayNegative = value;
        }

        public bool ShowCropRectangle
        {
            get => models.Display.ShowCropRectangle;
            set => models.Display.ShowCropRectangle = value;
        }

        public bool AllowMovieOverlay => models.Images.NumLayers > 1;

        public bool ShowMovieOverlay
        {
            get => models.Display.ActiveOverlay is MovieOverlay;
            set
            {
                if (value == ShowMovieOverlay) return;

                if (value)
                {
                    models.Display.ActiveOverlay = new MovieOverlay(models);
                }
                else if(models.Display.ActiveOverlay is MovieOverlay)
                {
                    models.Display.ActiveOverlay = null;
                }
            }
        }

        public bool FlipYAxis
        {
            get => models.Settings.FlipYAxis;
            set => models.Settings.FlipYAxis = value;
        }

        private IDisposable extendedView = null;

        public Visibility ExtendedViewVisibility => extendedView == null ? Visibility.Collapsed : Visibility.Visible;

        public ObservableCollection<ListItemViewModel<int>> AvailableMipMaps { get; } = new ObservableCollection<ListItemViewModel<int>>();
        public ObservableCollection<ListItemViewModel<int>> AvailableLayers { get; } = new ObservableCollection<ListItemViewModel<int>>();

        public ObservableCollection<ListItemViewModel<DisplayModel.SplitMode>> AvailableSplitModes { get; } = new ObservableCollection<ListItemViewModel<DisplayModel.SplitMode>>
        {
            new ListItemViewModel<DisplayModel.SplitMode>
            {
                Name = "Vertical",
                Cargo = DisplayModel.SplitMode.Vertical
            },
            new ListItemViewModel<DisplayModel.SplitMode>
            {
                Name = "Horizontal",
                Cargo = DisplayModel.SplitMode.Horizontal
            }
        };

        public ObservableCollection<ListItemViewModel<DisplayModel.ViewMode>> AvailableViewModes { get; } =
            new ObservableCollection<ListItemViewModel<DisplayModel.ViewMode>>();

        public bool EnableMipMaps => AvailableMipMaps.Count > 1;
        public Visibility EnableLayers => AvailableLayers.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EnableViewModes => AvailableViewModes.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public bool EnableSplitMode => models.NumEnabled == 2;

        // layers are fixed for cube maps
        public bool ChooseLayers => models.Display.ActiveView == DisplayModel.ViewMode.Single ||
                                    models.Display.ActiveView == DisplayModel.ViewMode.Polar;

        private ListItemViewModel<int> selectedMipMap = EmptyMipMap;
        public ListItemViewModel<int> SelectedMipMap
        {
            get => selectedMipMap;
            set
            {
                if (value == null || selectedMipMap == value) return;
                // determine active mipmap
                selectedMipMap = value;
                OnPropertyChanged(nameof(SelectedMipMap));
                if (selectedMipMap.Cargo != -1)
                    models.Display.ActiveMipmap = selectedMipMap.Cargo;
            }
        }

        public string Multiplier => models.Display.MultiplierString;

        private ListItemViewModel<int> selectedLayer = EmptyLayer;
        public ListItemViewModel<int> SelectedLayer
        {
            get => selectedLayer;
            set
            {
                if (value == null || selectedLayer == value) return;
                // determine active layer
                selectedLayer = value;
                OnPropertyChanged(nameof(SelectedLayer));
                if (selectedLayer.Cargo != -1)
                    models.Display.ActiveLayer = selectedLayer.Cargo;
            }
        }

        private ListItemViewModel<DisplayModel.SplitMode> selectedSplitMode;
        public ListItemViewModel<DisplayModel.SplitMode> SelectedSplitMode
        {
            get => selectedSplitMode;
            set
            {
                if (value == null || selectedSplitMode == value) return;
                selectedSplitMode = value;
                OnPropertyChanged(nameof(SelectedSplitMode));
                models.Display.Split = value.Cargo;
            }
        }

        private ListItemViewModel<DisplayModel.ViewMode> selectedViewMode = EmptyViewMode;
        public ListItemViewModel<DisplayModel.ViewMode> SelectedViewMode
        {
            get => selectedViewMode;
            set
            {
                if (value == null || selectedViewMode == value) return;
                // determine new view mode
                selectedViewMode = value;
                OnPropertyChanged(nameof(SelectedViewMode));
                models.Display.ActiveView = selectedViewMode.Cargo;
            }
        }

        public string Zoom
        {
            get => models.Display.ActiveView.IsDegree() ?
                        Math.Round((decimal)(models.Display.Aperture * 180.0 / Math.PI), 2).ToString(ImageFramework.Model.Models.Culture) + "°" :
                        Math.Round((decimal)(models.Display.Zoom * 100.0f), 2).ToString(ImageFramework.Model.Models.Culture) + "%";

            set
            {
                if (value == null) return;

                value = value.Trim();
                if (value.Length > 0 && (value.EndsWith("%") || value.EndsWith("°")))
                    value = value.Remove(value.Length - 1, 1);
                // extract float
                if (float.TryParse(value, NumberStyles.Float, ImageFramework.Model.Models.Culture, out float converted))
                {
                    if (models.Display.ActiveView.IsDegree())
                    {
                        models.Display.Aperture = (float)(converted * Math.PI / 180.0);
                    }
                    else
                    {
                        models.Display.Zoom = converted * 0.01f;
                    }
                }
                else
                {
                    // TODO do something
                }
            }
        }


        private int lastNumMipmaps = 0;
        private Size3 lastMipSize = Size3.Zero;
        private void CreateMipMapList()
        {
            if (lastNumMipmaps == models.Images.NumMipmaps &&
                lastMipSize == models.Images.Size) return; // list is already up to date

            var isEnabled = EnableMipMaps;
            AvailableMipMaps.Clear();
            for (var curMip = 0; curMip < models.Images.NumMipmaps; ++curMip)
            {
                var txt = models.Images.GetWidth(curMip) + "x" + models.Images.GetHeight(curMip);
                if (models.Images.Size.Depth > 1)
                    txt += "x" + models.Images.GetDepth(curMip);

                AvailableMipMaps.Add(new ListItemViewModel<int>
                {
                    Name = txt,
                    Cargo = curMip
                });
            }

            SelectedMipMap = AvailableMipMaps.Count != 0 ? AvailableMipMaps[0] : EmptyMipMap;

            lastNumMipmaps = models.Images.NumMipmaps;
            lastMipSize = models.Images.Size;

            OnPropertyChanged(nameof(AvailableMipMaps));
            if (isEnabled != EnableMipMaps)
                OnPropertyChanged(nameof(EnableMipMaps));
        }

        private void CreateLayersList()
        {
            var isEnabled = EnableLayers;
            AvailableLayers.Clear();
            for (var layer = 0; layer < models.Images.NumLayers; ++layer)
            {
                AvailableLayers.Add(new ListItemViewModel<int>
                {
                    Name = "Layer " + layer,
                    Cargo = layer
                });
            }

            SelectedLayer = AvailableLayers.Count != 0 ? AvailableLayers[0] : EmptyLayer;

            OnPropertyChanged(nameof(AvailableLayers));
            if (isEnabled != EnableLayers)
                OnPropertyChanged(nameof(EnableLayers));
        }

        private void CreateViewModes()
        {
            var isEnabled = EnableViewModes;
            AvailableViewModes.Clear();

            var selected = EmptyViewMode;
            foreach (var view in models.Display.AvailableViews)
            {
                var box = new ListItemViewModel<DisplayModel.ViewMode>
                {
                    Name = view.ToString(),
                    Cargo = view
                };
                if (view == models.Display.ActiveView)
                    selected = box;

                AvailableViewModes.Add(box);
            }

            SelectedViewMode = selected;

            OnPropertyChanged(nameof(AvailableViewModes));
            if (isEnabled != EnableViewModes)
                OnPropertyChanged(nameof(EnableViewModes));
        }

        public string TexelPosition
        {
            get
            {
                if (!models.Display.TexelPosition.HasValue) return "X";
                var tp = models.Display.TexelPosition.Value;

                var res = tp.X + ", " + GetTexelPositionY(tp.Y);
                if (models.Images.ImageType == typeof(Texture3D))
                    res += ", " + tp.Z;
                return res;
            }
        }
           

        public Visibility DxSystemVisibility =>
            models.Settings.FlipYAxis ? Visibility.Collapsed : Visibility.Visible;

        public Visibility GlSystemVisibility =>
            models.Settings.FlipYAxis ? Visibility.Visible : Visibility.Collapsed;

        public bool IsAlphaWhite
        {
            get => models.Settings.AlphaBackground == SettingsModel.AlphaType.White;
            set
            {
                if(value) models.Settings.AlphaBackground = SettingsModel.AlphaType.White;
            } 
        }
        public bool IsAlphaBlack
        {
            get => models.Settings.AlphaBackground == SettingsModel.AlphaType.Black;
            set
            {
                if (value) models.Settings.AlphaBackground = SettingsModel.AlphaType.Black;
            }
        }
        public bool IsAlphaCheckers
        {
            get => models.Settings.AlphaBackground == SettingsModel.AlphaType.Checkers;
            set
            {
                if (value) models.Settings.AlphaBackground = SettingsModel.AlphaType.Checkers;
            }
        }
        public bool IsAlphaTheme
        {
            get => models.Settings.AlphaBackground == SettingsModel.AlphaType.Theme;
            set
            {
                if (value) models.Settings.AlphaBackground = SettingsModel.AlphaType.Theme;
            }
        }

        /// <summary>
        /// respects flipping of texel coordinate
        /// </summary>
        private int GetTexelPositionY(int value)
        {
            if (!models.Settings.FlipYAxis) return value;
            if (models.Images.NumImages == 0) return 0;
            return models.Images.GetHeight(models.Display.ActiveMipmap) - value - 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
