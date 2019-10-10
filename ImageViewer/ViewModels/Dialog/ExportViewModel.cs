using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Annotations;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Export;
using ImageViewer.Models;
using ImageViewer.Views;

namespace ImageViewer.ViewModels.Dialog
{
    public class ExportViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ModelsEx models;
        private readonly string filename;
        private readonly string extension;

        public ExportViewModel(ModelsEx models, string extension, GliFormat preferredFormat, string filename)
        {
            this.models = models;
            this.extension = extension;
            this.filename = filename;
            models.Export.IsExporting = true;
            Quality = models.Settings.LastQuality;

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

            // all layer option for ktx and dds
            if (models.Images.NumLayers > 1 && (extension == "ktx" || extension == "dds"))
            {
                AvailableLayers.Add(new ComboBoxItem<int>("All Layer", -1));
                selectedLayer = AvailableLayers.Last();
                models.Export.Layer = selectedLayer.Cargo;
            }

            // all mipmaps option for ktx and dds
            if (models.Images.NumMipmaps > 1 && (extension == "ktx" || extension == "dds"))
            {
                AvailableMipmaps.Add(new ComboBoxItem<int>("All Mipmaps", -1));
                selectedMipmap = AvailableMipmaps.Last();
                models.Export.Mipmap = selectedMipmap.Cargo;
            }

            // init formats
            var usedFormat = models.Export.Formats.First(fmt => fmt.Extension == extension);

            foreach (var format in usedFormat.Formats)
            {
                AvailableFormat.Add(new ComboBoxItem<GliFormat>(format.ToString(), format));
                if (format == preferredFormat)
                    SelectedFormat = AvailableFormat.Last();
            }

            models.Export.PropertyChanged += ExportOnPropertyChanged;
        }

        public void Dispose()
        {
            models.Export.PropertyChanged -= ExportOnPropertyChanged;
            models.Settings.LastQuality = Quality;
            models.Export.IsExporting = false;
        }

        private void ExportOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ExportModel.UseCropping):
                    OnPropertyChanged(nameof(UseCropping));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.CropStartX):
                    OnPropertyChanged(nameof(CropStartX));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.CropStartY):
                    OnPropertyChanged(nameof(CropStartY));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.CropEndX):
                    OnPropertyChanged(nameof(CropEndX));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.CropEndY):
                    OnPropertyChanged(nameof(CropEndY));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.Mipmap):
                    if (models.Export.Mipmap < 0)
                        selectedMipmap = AvailableMipmaps.Last();
                    else
                        selectedMipmap = AvailableMipmaps[models.Export.Mipmap];
                    OnPropertyChanged(nameof(SelectedMipmap));
                    break;
                case nameof(ExportModel.Layer):
                    if (models.Export.Layer < 0)
                        selectedLayer = AvailableLayers.Last();
                    else
                        selectedLayer = AvailableLayers[models.Export.Layer];
                    OnPropertyChanged(nameof(SelectedLayer));
                    break;
                case nameof(ExportModel.Quality):
                    OnPropertyChanged(nameof(Quality));
                    break;
                case nameof(ExportModel.AllowCropping):
                    OnPropertyChanged(nameof(AllowCropping));
                    break;
            }
        }

        public bool IsValid => !UseCropping || (CropStartX <= CropEndX && CropStartY <= CropEndY);

        public string Filename
        {
            get => filename;
            set
            {
                // do nothing. the text box needs a read/write property but wont be changed anyways
            }
        }

        public ObservableCollection<ComboBoxItem<int>> AvailableLayers { get; } = new ObservableCollection<ComboBoxItem<int>>();
        public ObservableCollection<ComboBoxItem<int>> AvailableMipmaps { get; } = new ObservableCollection<ComboBoxItem<int>>();
        public ObservableCollection<ComboBoxItem<GliFormat>> AvailableFormat { get; } = new ObservableCollection<ComboBoxItem<GliFormat>>();

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
                OnPropertyChanged(nameof(CropMaxX));
                OnPropertyChanged(nameof(CropMaxY));
            }
        }

        private ComboBoxItem<GliFormat> selectedFormat;
        public ComboBoxItem<GliFormat> SelectedFormat
        {
            get => selectedFormat;
            set
            {
                if (value == null || value == selectedFormat) return;
                selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        public GliFormat SelectedFormatValue => selectedFormat.Cargo;

        public bool UseCropping
        {
            get => models.Export.UseCropping;
            set => models.Export.UseCropping = value;
        }

        public bool AllowCropping => models.Export.AllowCropping;

        public int CropMinX => 0;
        public int CropMaxX => models.Images.GetWidth(Math.Max(selectedMipmap.Cargo, 0));
        public int CropMinY => 0;
        public int CropMaxY => models.Images.GetHeight(Math.Max(selectedMipmap.Cargo, 0));

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

        public Visibility HasQuality => extension == "jpg" ? Visibility.Visible : Visibility.Collapsed;
        public int MinQuality => ExportModel.QualityMin;
        public int MaxQuality => ExportModel.QualityMax;
        public int Quality
        {
            get => models.Export.Quality;
            set => models.Export.Quality = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
