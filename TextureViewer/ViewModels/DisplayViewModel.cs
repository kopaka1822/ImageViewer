using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private static readonly string emptyMipMap = "No Mipmap";
        private static readonly string emptyLayer = "No Layer";

        public DisplayViewModel(Models.Models models)
        {
            this.models = models;
            models.Display.PropertyChanged += DisplayModelOnPropertyChanged;
            models.Images.PropertyChanged += ImagesModelOnPropertyChanged;
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

        public ObservableCollection<string> AvailableMipMaps { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableLayers { get; } = new ObservableCollection<string>();

        public Visibility EnableMipMaps => AvailableMipMaps.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility EnableLayers => AvailableLayers.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

        private string selectedMipMap = emptyMipMap;
        public string SelectedMipMap
        {
            get => selectedMipMap;
            set
            {
                if (selectedMipMap == value) return;
                // determine active mipmap
                selectedMipMap = value;
                OnPropertyChanged(nameof(SelectedMipMap));
            }
        }

        private string selectedLayer = emptyLayer;
        public string SelectedLayer
        {
            get => selectedLayer;
            set
            {
                if (selectedLayer == value) return;
                // determine active layer
                selectedLayer = value;
                OnPropertyChanged(nameof(SelectedLayer));
            }
        }

        private void CreateMipMapList()
        {
            var isEnabled = EnableMipMaps;
            AvailableMipMaps.Clear();
            for (var curMip = 0; curMip < models.Images.NumMipmaps; ++curMip)
            {
                AvailableMipMaps.Add(models.Images.GetWidth(curMip) + "x" + models.Images.GetHeight(curMip));
            }

            SelectedMipMap = AvailableMipMaps.Count != 0 ? AvailableMipMaps[0] : emptyMipMap;

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
                AvailableLayers.Add("Layer " + layer);
            }

            SelectedLayer = AvailableLayers.Count != 0 ? AvailableLayers[0] : emptyLayer;

            OnPropertyChanged(nameof(AvailableLayers));
            if(isEnabled != EnableLayers)
                OnPropertyChanged(nameof(EnableLayers));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
