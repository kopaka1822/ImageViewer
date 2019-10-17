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
using ImageFramework.Utility;
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

            // set crop borders


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
                AvailableFormat.Add(new ComboBoxItem<GliFormat>(format.ToString(), format, format.GetDescription()));
                if (format == preferredFormat)
                    SelectedFormat = AvailableFormat.Last();
            }

            if (SelectedFormat == null)
                SelectedFormat = AvailableFormat[0];

            // enable quality
            if (extension == "jpg")
            {
                hasQualityValue = true;
            }
            else SetKtxDdsQuality();

            models.Export.PropertyChanged += ExportOnPropertyChanged;

            if (models.Export.CropEndX == 0 && models.Export.CropEndY == 0)
            {
                // assume cropping was not set
                SetMaxCropping();
            }
        }

        private void SetKtxDdsQuality()
        {
            if (extension == "ktx" || extension == "dds")
            {
                HasQualityValue = SelectedFormat.Cargo.IsCompressed();
            }
        }

        private void SetMaxCropping()
        {
            CropStartX = 0;
            CropStartY = 0;
            CropEndX = CropMaxX;
            CropEndY = CropMaxY;
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
                    OnPropertyChanged(nameof(CropEndY));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.CropEndX):
                    OnPropertyChanged(nameof(CropEndX));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.CropEndY):
                    OnPropertyChanged(nameof(CropEndY));
                    OnPropertyChanged(nameof(CropStartY));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportModel.Mipmap):
                    if (models.Export.Mipmap < 0)
                        selectedMipmap = AvailableMipmaps.Last();
                    else
                        selectedMipmap = AvailableMipmaps[models.Export.Mipmap];
                    OnPropertyChanged(nameof(SelectedMipmap));
                    OnPropertyChanged(nameof(UseCropping));
                    OnPropertyChanged(nameof(CropMaxX));
                    OnPropertyChanged(nameof(CropMaxY));
                    // refit start and end since dimensions changed
                    CropStartX = CropStartX;
                    CropStartY = CropStartY;
                    CropEndX = CropEndX;
                    CropEndY = CropEndY;
                    // force change on y components because coordinate flipping
                    OnPropertyChanged(nameof(CropStartY));
                    OnPropertyChanged(nameof(CropEndY));
                    OnPropertyChanged(nameof(IsValid));
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
                
                // preview layer
                if(value.Cargo >= 0)
                    models.Display.ActiveLayer = value.Cargo;
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
                // preview mipmap
                models.Display.ActiveMipmap = Math.Max(value.Cargo, 0);
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
                OnPropertyChanged(nameof(Description));
                SetKtxDdsQuality();
            }
        }

        public GliFormat SelectedFormatValue => selectedFormat.Cargo;

        public string Description => selectedFormat.Cargo.GetDescription();

        public bool UseCropping
        {
            get => models.Export.UseCropping;
            set => models.Export.UseCropping = value;
        }

        public bool AllowCropping => models.Export.AllowCropping;

        public int CropMinX => 0;
        public int CropMaxX => models.Images.GetWidth(Math.Max(selectedMipmap.Cargo, 0)) - 1;
        public int CropMinY => 0;
        public int CropMaxY => models.Images.GetHeight(Math.Max(selectedMipmap.Cargo, 0)) - 1;

        public int CropStartX
        {
            get => models.Export.CropStartX;
            set
            {
                var clamped = Utility.Clamp(value, CropMinX, CropMaxX);
                models.Export.CropStartX = clamped;

                if(clamped != value) OnPropertyChanged(nameof(CropStartX));
                CropEndX = CropEndX; // maybe adjust this value
            }
        }

        public int CropStartY
        {
            get => models.Settings.FlipYAxis ? FlipY(models.Export.CropEndY) : models.Export.CropStartY;
            set
            {
                var clamped = Utility.Clamp(value, CropMinY, CropMaxY);
                if (models.Settings.FlipYAxis)
                {
                    // set crop end y
                    models.Export.CropEndY = FlipY(clamped);
                }
                else
                {
                    // set crop start y
                    models.Export.CropStartY = clamped;
                }

                if (clamped != value) OnPropertyChanged(nameof(CropStartY));
            }
        }

        public int CropEndX
        {
            get => models.Export.CropEndX;
            set
            {
                var clamped = Utility.Clamp(value, CropMinX, CropMaxX);
                models.Export.CropEndX = clamped;

                if(clamped != value) OnPropertyChanged(nameof(CropEndX));
            }
        }

        public int CropEndY
        {
            get => models.Settings.FlipYAxis ? FlipY(models.Export.CropStartY) :  models.Export.CropEndY;
            set
            {
                var clamped = Utility.Clamp(value, CropMinY, CropMaxY);
                if (models.Settings.FlipYAxis)
                {
                    // set crop start y
                    models.Export.CropStartY = FlipY(clamped);
                }
                else
                {
                    models.Export.CropEndY = clamped;
                }

                if(clamped != value) OnPropertyChanged(nameof(CropEndY));
            }
        }

        private int FlipY(int value)
        {
            return CropMaxY - value;
        }

        private bool hasQualityValue = false;

        private bool HasQualityValue
        {
            get => hasQualityValue;
            set
            {
                if(value == hasQualityValue) return;
                hasQualityValue = value;
                OnPropertyChanged(nameof(HasQuality));
            }
        }

        public Visibility HasQuality => HasQualityValue ? Visibility.Visible : Visibility.Collapsed;
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
