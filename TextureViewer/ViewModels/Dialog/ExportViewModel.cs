using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Annotations;
using TextureViewer.Models.Dialog;
using TextureViewer.Views;

namespace TextureViewer.ViewModels.Dialog
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;

        public ExportViewModel(Models.Models models)
        {
            this.models = models;

            // init layers
            for (var i = 0; i < models.Images.NumLayers; ++i)
            {
                AvailableLayers.Add(new ComboBoxItem<int>("Layer " + i, i));
            }
            selectedLayer = AvailableLayers[models.Export.Layer];
            Debug.Assert(selectedLayer.Cargo == models.Export.Layer);

            // init mipmaps
            for (var i = 0; i < models.Images.NumMipmaps; ++i)
            {
                AvailableMipmaps.Add(new ComboBoxItem<int>("Mipmap " + i, i));
            }
            selectedMipmap = AvailableMipmaps[models.Export.Mipmap];
            Debug.Assert(selectedMipmap.Cargo == models.Export.Mipmap);

            // init formats
            foreach (var format in models.Export.SupportedFormats)
            {
                AvailableFormat.Add(new ComboBoxItem<PixelFormat>(format.ToString().ToUpper(), format));
                if (format == models.Export.PixelFormat)
                    SelectedFormat = AvailableFormat.Last();
            }

            models.Export.PropertyChanged += ExportOnPropertyChanged;
        }

        public void Dispose()
        {
            models.Export.PropertyChanged -= ExportOnPropertyChanged;
        }

        private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ExportModel.UseCropping):
                    OnPropertyChanged(nameof(UseCropping));
                    break;
                case nameof(ExportModel.CropMinX):
                    OnPropertyChanged(nameof(CropMinX));
                    break;
                case nameof(ExportModel.CropMinY):
                    OnPropertyChanged(nameof(CropMinY));
                    break;
                case nameof(ExportModel.CropMaxX):
                    OnPropertyChanged(nameof(CropMaxX));
                    break;
                case nameof(ExportModel.CropMaxY):
                    OnPropertyChanged(nameof(CropMaxY));
                    break;
                case nameof(ExportModel.CropStartX):
                    OnPropertyChanged(nameof(CropStartX));
                    break;
                case nameof(ExportModel.CropStartY):
                    OnPropertyChanged(nameof(CropStartY));
                    break;
                case nameof(ExportModel.CropEndX):
                    OnPropertyChanged(nameof(CropEndX));
                    break;
                case nameof(ExportModel.CropEndY):
                    OnPropertyChanged(nameof(CropEndY));
                    break;
                case nameof(ExportModel.Mipmap):
                    selectedMipmap = AvailableMipmaps[models.Export.Mipmap];
                    OnPropertyChanged(nameof(SelectedMipmap));
                    break;
                case nameof(ExportModel.Layer):
                    selectedLayer = AvailableLayers[models.Export.Layer];
                    OnPropertyChanged(nameof(SelectedLayer));
                    break;
            }
        }

        public string Filename
        {
            get => models.Export.Filename;
            set
            {
                // do nothing. the text box needs a read/write property but wont be changed anyways
            }
        }

        public ObservableCollection<ComboBoxItem<int>> AvailableLayers { get; } = new ObservableCollection<ComboBoxItem<int>>();
        public ObservableCollection<ComboBoxItem<int>> AvailableMipmaps { get; } = new ObservableCollection<ComboBoxItem<int>>();
        public ObservableCollection<ComboBoxItem<PixelFormat>> AvailableFormat { get; } = new ObservableCollection<ComboBoxItem<PixelFormat>>();

        public bool EnableLayers => AvailableLayers.Count > 1;
        public bool EnableMipmaps => AvailableMipmaps.Count > 1;
        public bool EnableFormat => AvailableFormat.Count > 1;

        private ComboBoxItem<int> selectedLayer;
        public ComboBoxItem<int> SelectedLayer
        {
            get => selectedLayer;
            set
            {
                if (value == null || value == selectedLayer) return;
                //selectedLayer = value;
                models.Export.Layer = value.Cargo;
                //OnPropertyChanged(nameof(SelectedLayer));
            }
        }

        private ComboBoxItem<int> selectedMipmap;
        public ComboBoxItem<int> SelectedMipmap
        {
            get => selectedMipmap;
            set
            {
                if (value == null || value == selectedMipmap) return;
                //selectedMipmap = value;
                models.Export.Mipmap = value.Cargo;
                //OnPropertyChanged(nameof(SelectedMipmap));
            }
        }

        private ComboBoxItem<PixelFormat> selectedFormat;
        public ComboBoxItem<PixelFormat> SelectedFormat
        {
            get => selectedFormat;
            set
            {
                if (value == null || value == selectedFormat) return;
                selectedFormat = value;
                models.Export.PixelFormat = selectedFormat.Cargo;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        public bool UseCropping
        {
            get => models.Export.UseCropping;
            set => models.Export.UseCropping = value;
        }

        public int CropMinX => models.Export.CropMinX;
        public int CropMaxX => models.Export.CropMaxX;
        public int CropMinY => models.Export.CropMinY;
        public int CropMaxY => models.Export.CropMaxY;

        public int CropStartX
        {
            get => models.Export.CropStartX;
            set => models.Export.CropStartX = value;
        }

        public int CropStartY
        {
            get => models.Export.CropStartY;
            set => models.Export.CropStartY = value;
        }

        public int CropEndX
        {
            get => models.Export.CropEndX;
            set => models.Export.CropEndX = value;
        }

        public int CropEndY
        {
            get => models.Export.CropEndY;
            set => models.Export.CropEndY = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
