using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ModelViews
{
    public class DisplayViewModel : INotifyPropertyChanged
    {
        private DisplayModel displayModel;
        private ImagesModel imagesModel;
        private static readonly string emptyMipMap = "No Mipmap";
        private static readonly string emptyLayer = "No Layer";

        public DisplayViewModel(DisplayModel displayModel, ImagesModel imagesModel)
        {
            this.displayModel = displayModel;
            this.imagesModel = imagesModel;

            imagesModel.PropertyChanged += ImagesModelOnPropertyChanged;
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

        public ObservableCollection<string> AvailableMipMaps { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableLayers { get; } = new ObservableCollection<string>();

        public Visibility EnableMipMaps => AvailableMipMaps.Count > 1 ? Visibility.Visible : Visibility.Hidden;
        public Visibility EnableLayers => AvailableLayers.Count > 1 ? Visibility.Visible : Visibility.Hidden;

        private string selectedMipMap = emptyMipMap;
        public string SelectedMipMap
        {
            get => selectedMipMap;
            set
            {
                if (selectedMipMap == value) return;
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
                selectedLayer = value;
                OnPropertyChanged(nameof(SelectedLayer));
            }
        }

        private void CreateMipMapList()
        {
            var isEnabled = EnableMipMaps;
            AvailableMipMaps.Clear();
            for (var curMip = 0; curMip < imagesModel.NumMipmaps; ++curMip)
            {
                AvailableMipMaps.Add(imagesModel.GetWidth(curMip) + "x" + imagesModel.GetHeight(curMip));
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
            for (var layer = 0; layer < imagesModel.NumLayers; ++layer)
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
