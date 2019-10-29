using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageViewer.Models;
using ImageViewer.Views;

namespace ImageViewer.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private static readonly ComboBoxItem<int> EmptyMipMap = new ComboBoxItem<int>("No Mipmap", -1);
        private static readonly ComboBoxItem<int> EmptyLayer = new ComboBoxItem<int>("No Layer", -1);
        private static readonly ComboBoxItem<DisplayModel.ViewMode> EmptyViewMode = new ComboBoxItem<DisplayModel.ViewMode>("Empty", DisplayModel.ViewMode.Empty);

        public DisplayViewModel(ModelsEx models)
        {
            this.models = models;
            this.selectedSplitMode = AvailableSplitModes[models.Display.Split == DisplayModel.SplitMode.Vertical ? 0 : 1];
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
            }
        }

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

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumMipmaps):
                case nameof(ImagesModel.Width):
                case nameof(ImagesModel.Height):
                    CreateMipMapList();
                    break;
                case nameof(ImagesModel.NumLayers):
                    CreateLayersList();
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

        public bool FlipYAxis
        {
            get => models.Settings.FlipYAxis;
            set => models.Settings.FlipYAxis = value;
        }

        public ObservableCollection<ComboBoxItem<int>> AvailableMipMaps { get; } = new ObservableCollection<ComboBoxItem<int>>();
        public ObservableCollection<ComboBoxItem<int>> AvailableLayers { get; } = new ObservableCollection<ComboBoxItem<int>>();

        public ObservableCollection<ComboBoxItem<DisplayModel.SplitMode>> AvailableSplitModes { get; } = new ObservableCollection<ComboBoxItem<DisplayModel.SplitMode>>
        {
            new ComboBoxItem<DisplayModel.SplitMode>("Vertical", DisplayModel.SplitMode.Vertical),
            new ComboBoxItem<DisplayModel.SplitMode>("Horizontal", DisplayModel.SplitMode.Horizontal)
        };

        public ObservableCollection<ComboBoxItem<DisplayModel.ViewMode>> AvailableViewModes { get; } =
            new ObservableCollection<ComboBoxItem<DisplayModel.ViewMode>>();

        public bool EnableMipMaps => AvailableMipMaps.Count > 1;
        public Visibility EnableLayers => AvailableLayers.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EnableViewModes => AvailableViewModes.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public bool EnableSplitMode => models.NumEnabled == 2;

        // layers are fixed for cube maps
        public bool ChooseLayers => models.Display.ActiveView == DisplayModel.ViewMode.Single ||
                                    models.Display.ActiveView == DisplayModel.ViewMode.Polar;

        private ComboBoxItem<int> selectedMipMap = EmptyMipMap;
        public ComboBoxItem<int> SelectedMipMap
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

        private ComboBoxItem<int> selectedLayer = EmptyLayer;
        public ComboBoxItem<int> SelectedLayer
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

        private ComboBoxItem<DisplayModel.SplitMode> selectedSplitMode;
        public ComboBoxItem<DisplayModel.SplitMode> SelectedSplitMode
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

        private ComboBoxItem<DisplayModel.ViewMode> selectedViewMode = EmptyViewMode;
        public ComboBoxItem<DisplayModel.ViewMode> SelectedViewMode
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
                        Math.Round((Decimal)(models.Display.Aperture * 180.0 / Math.PI), 2).ToString(ImageFramework.Model.Models.Culture) + "°" :
                        Math.Round((Decimal)(models.Display.Zoom * 100.0f), 2).ToString(ImageFramework.Model.Models.Culture) + "%";

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
        private int lastMipWidth = 0;
        private int lastMipHeight = 0;
        private void CreateMipMapList()
        {
            if (lastNumMipmaps == models.Images.NumMipmaps &&
                lastMipWidth == models.Images.Width &&
                lastMipHeight == models.Images.Height) return; // list is already up to date

            var isEnabled = EnableMipMaps;
            AvailableMipMaps.Clear();
            for (var curMip = 0; curMip < models.Images.NumMipmaps; ++curMip)
            {
                AvailableMipMaps.Add(new ComboBoxItem<int>(models.Images.GetWidth(curMip) + "x" + models.Images.GetHeight(curMip), curMip));
            }

            SelectedMipMap = AvailableMipMaps.Count != 0 ? AvailableMipMaps[0] : EmptyMipMap;

            lastNumMipmaps = models.Images.NumMipmaps;
            lastMipWidth = models.Images.Width;
            lastMipHeight = models.Images.Height;

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
                AvailableLayers.Add(new ComboBoxItem<int>("Layer " + layer, layer));
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

            ComboBoxItem<DisplayModel.ViewMode> selected = EmptyViewMode;
            foreach (var view in models.Display.AvailableViews)
            {
                var box = new ComboBoxItem<DisplayModel.ViewMode>(view.ToString(), view);
                if (view == models.Display.ActiveView)
                    selected = box;

                AvailableViewModes.Add(box);
            }

            SelectedViewMode = selected;

            OnPropertyChanged(nameof(AvailableViewModes));
            if (isEnabled != EnableViewModes)
                OnPropertyChanged(nameof(EnableViewModes));
        }

        public string TexelPosition => models.Display.TexelPosition.X + ", " + GetTexelPositionY();

        public Visibility DxSystemVisibility =>
            models.Settings.FlipYAxis ? Visibility.Collapsed : Visibility.Visible;

        public Visibility GlSystemVisibility =>
            models.Settings.FlipYAxis ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// respects flipping of texel coordinate
        /// </summary>
        private int GetTexelPositionY()
        {
            if (!models.Settings.FlipYAxis) return models.Display.TexelPosition.Y;
            if (models.Images.NumImages == 0) return 0;
            return models.Images.GetHeight(models.Display.ActiveMipmap) - models.Display.TexelPosition.Y - 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
