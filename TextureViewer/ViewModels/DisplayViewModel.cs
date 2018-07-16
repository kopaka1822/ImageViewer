using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Models;
using TextureViewer.Views;

namespace TextureViewer.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private static readonly ComboBoxItem<int> EmptyMipMap = new ComboBoxItem<int>("No Mipmap", -1);
        private static readonly ComboBoxItem<int> EmptyLayer = new ComboBoxItem<int>("No Layer", -1);
        private static readonly ComboBoxItem<DisplayModel.ViewMode> EmptyViewMode = new ComboBoxItem<DisplayModel.ViewMode>("Empty", DisplayModel.ViewMode.Empty);

        public DisplayViewModel(Models.Models models)
        {
            this.models = models;
            this.selectedSplitMode = AvailableSplitModes[models.Display.Split == DisplayModel.SplitMode.Vertical ? 0 : 1];
            models.Display.PropertyChanged += DisplayModelOnPropertyChanged;
            models.Images.PropertyChanged += ImagesModelOnPropertyChanged;
            models.Equations.PropertyChanged += EquationsOnPropertyChanged;

            CreateViewModes();
        }

        private void EquationsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationsModel.NumVisible):
                    OnPropertyChanged(nameof(EnableSplitMode));
                    break;
            }
        }

        private void DisplayModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.LinearInterpolation):
                    OnPropertyChanged(nameof(LinearInterpolation));
                    break;

                case nameof(DisplayModel.Grayscale):
                    // assume that everything has changed
                    OnPropertyChanged(nameof(IsGrayscaleDisabled));
                    OnPropertyChanged(nameof(IsGrayscaleRed));
                    OnPropertyChanged(nameof(IsGrayscaleGreen));
                    OnPropertyChanged(nameof(IsGrayscaleBlue));
                    OnPropertyChanged(nameof(IsGrayscaleAlpha));
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
            }
        }

        private void ImagesModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                // if the number of mipmaps has changed recreate all lists
                case nameof(ImagesModel.NumMipmaps):
                    CreateMipMapList();
                    CreateLayersList();
                    break;
              
            }
        }

        public bool LinearInterpolation
        {
            get => models.Display.LinearInterpolation;
            set => models.Display.LinearInterpolation = value;
        }

        public bool IsGrayscaleDisabled
        {
            get => models.Display.Grayscale == DisplayModel.GrayscaleMode.Disabled;
            set
            {
                if (value)
                    models.Display.Grayscale = DisplayModel.GrayscaleMode.Disabled;
            }
        }

        public bool IsGrayscaleRed
        {
            get => models.Display.Grayscale == DisplayModel.GrayscaleMode.Red;
            set
            {
                if (value)
                    models.Display.Grayscale = DisplayModel.GrayscaleMode.Red;
            }
        }

        public bool IsGrayscaleGreen
        {
            get => models.Display.Grayscale == DisplayModel.GrayscaleMode.Green;
            set
            {
                if (value)
                    models.Display.Grayscale = DisplayModel.GrayscaleMode.Green;
            }
        }

        public bool IsGrayscaleBlue
        {
            get => models.Display.Grayscale == DisplayModel.GrayscaleMode.Blue;
            set
            {
                if (value)
                    models.Display.Grayscale = DisplayModel.GrayscaleMode.Blue;
            }
        }

        public bool IsGrayscaleAlpha
        {
            get => models.Display.Grayscale == DisplayModel.GrayscaleMode.Alpha;
            set
            {
                if(value)
                    models.Display.Grayscale = DisplayModel.GrayscaleMode.Alpha;
            }
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
        public bool EnableSplitMode => models.Equations.GetVisibles().Count == 2;

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
                        Math.Round((Decimal)(models.Display.Aperture  * 180.0 / Math.PI), 2 ).ToString(App.GetCulture()) + "°" :
                        Math.Round((Decimal) (models.Display.Zoom * 100.0f), 2).ToString(App.GetCulture()) + "%";
               
            set
            {
                if (value == null) return;

                value = value.Trim();
                if (value.Length > 0 && (value.EndsWith("%") || value.EndsWith("°")))
                    value = value.Remove(value.Length - 1, 1);
                // extract float
                if (float.TryParse(value, NumberStyles.Float, App.GetCulture(), out float converted))
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

        private void CreateMipMapList()
        {
            var isEnabled = EnableMipMaps;
            AvailableMipMaps.Clear();
            for (var curMip = 0; curMip < models.Images.NumMipmaps; ++curMip)
            {
                AvailableMipMaps.Add(new ComboBoxItem<int>(models.Images.GetWidth(curMip) + "x" + models.Images.GetHeight(curMip), curMip));
            }

            SelectedMipMap = AvailableMipMaps.Count != 0 ? AvailableMipMaps[0] : EmptyMipMap;

            OnPropertyChanged(nameof(AvailableMipMaps));
            if(isEnabled != EnableMipMaps)
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
            if(isEnabled != EnableLayers)
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
            if(isEnabled != EnableViewModes)
                OnPropertyChanged(nameof(EnableViewModes));
        }

        public string TexelPosition => models.Display.TexelPosition.X + ", " + models.Display.TexelPosition.Y;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
