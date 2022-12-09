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
using System.Windows.Documents;
using ImageFramework.Annotations;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Export;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.Views;

namespace ImageViewer.ViewModels.Dialog
{
    public class ExportViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ModelsEx models;
        private readonly ExportFormatModel usedFormat;
        private readonly string filename;
        private readonly string extension;
        private readonly bool is3D;
        private readonly List<ListItemViewModel<GliFormat>> allFormats = new List<ListItemViewModel<GliFormat>>();
        private readonly List<int> formatRatings = new List<int>();
        // warning if exporting into non srgb formats (ldr file formats)
        private readonly bool nonSrgbExportWarnings = false;

        public ExportViewModel(ModelsEx models, string extension, GliFormat preferredFormat, string filename, bool is3D, DefaultStatistics stats)
        {
            this.models = models;
            this.extension = extension;
            this.filename = filename;
            this.is3D = is3D;
            this.usedFormat = ExportDescription.GetExportFormat(extension);
            if(usedFormat == null)
                throw new Exception("unsupported export format: " + extension);
            this.Crop = models.ExportConfig.GetViewModel(models);
            models.Display.IsExporting = true;

            // init layers
            for (var i = 0; i < models.Images.NumLayers; ++i)
            {
                AvailableLayers.Add(new ListItemViewModel<int>
                {
                    Cargo = i,
                    Name = "Layer " + i
                });
            }

            models.ExportConfig.Layer = models.Display.ActiveLayer;
            selectedLayer = AvailableLayers[models.Display.ActiveLayer];
            Debug.Assert(selectedLayer.Cargo == models.Display.ActiveLayer);

            // init mipmaps
            for (var i = 0; i < models.Images.NumMipmaps; ++i)
            {
                AvailableMipmaps.Add(new ListItemViewModel<int>
                {
                    Cargo = i,
                    Name = "Mipmap " + i
                });
            }

            models.ExportConfig.Mipmap = models.Display.ActiveMipmap;
            selectedMipmap = AvailableMipmaps[models.Display.ActiveMipmap];
            Debug.Assert(selectedMipmap.Cargo == models.Display.ActiveMipmap);

            // all layer option for ktx and dds
            if (models.Images.NumLayers > 1 && (extension == "ktx" || extension == "dds" || extension == "ktx2"))
            {
                AvailableLayers.Add(new ListItemViewModel<int>
                {
                    Cargo = -1,
                    Name = "All Layer"
                });
                selectedLayer = AvailableLayers.Last();
                models.ExportConfig.Layer = -1;
            }

            // all mipmaps option for ktx and dds
            if (models.Images.NumMipmaps > 1 && (extension == "ktx" || extension == "dds" || extension == "ktx2"))
            {
                AvailableMipmaps.Add(new ListItemViewModel<int>
                {
                    Cargo = -1,
                    Name = "All Mipmaps"
                });
                selectedMipmap = AvailableMipmaps.Last();
                models.ExportConfig.Mipmap = -1;
            }

            // init available pixel data types
            var usedPixelTypes = new SortedSet<PixelDataType>();
            foreach (var format in usedFormat.Formats)
            {
                // exclude some formats for 3d export
                if(is3D && format.IsExcludedFrom3DExport()) continue;

                allFormats.Add(new ListItemViewModel<GliFormat>
                {
                    Cargo = format,
                    Name = format.ToString(),
                    ToolTip = format.GetDescription()
                });
                formatRatings.Add(stats.GetFormatRating(format, preferredFormat));
                usedPixelTypes.Add(format.GetDataType());
            }

            if(usedPixelTypes.Count > 1)
                AvailableDataTypes.Add(new ListItemViewModel<PixelDataType>
                {
                    Cargo = PixelDataType.Undefined,
                    Name = "All"
                });
            var preferredPixelType = preferredFormat.GetDataType();
            foreach (var usedPixelType in usedPixelTypes)
            {
                AvailableDataTypes.Add(new ListItemViewModel<PixelDataType>
                {
                    Cargo = usedPixelType,
                    Name = usedPixelType.ToString(),
                    ToolTip = usedPixelType.GetDescription()
                });
                if (usedPixelType == preferredPixelType)
                    SelectedDataType = AvailableDataTypes.Last();
            }

            if (SelectedDataType == null)
                SelectedDataType = AvailableDataTypes[0];

            // assert that those were were set by SelectedDataType
            Debug.Assert(AvailableFormats != null);
            Debug.Assert(SelectedFormat != null);

            // enable quality
            hasQualityValue = usedFormat.SupportsQuality(SelectedFormat.Cargo);
            models.ExportConfig.PropertyChanged += ExportConfigOnPropertyChanged;
            models.Settings.PropertyChanged += SettingsOnPropertyChanged;

            // cropping adjustments
            Crop.Mipmap = selectedMipmap.Cargo;
            if (models.ExportConfig.CropEnd == Float3.Zero)
            {
                // assume cropping was not set
                models.ExportConfig.SetMaxCropping();
            }
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.LastQuality):
                    OnPropertyChanged(nameof(Quality));
                    break;
                case nameof(SettingsModel.ExportZoomBoxBorder):
                    OnPropertyChanged(nameof(ZoomBorders));
                    break;
                case nameof(SettingsModel.ExportZoomBoxScale):
                    OnPropertyChanged(nameof(ZoomBoxScale));
                    break;
            }
        }

        public void Dispose()
        {
            models.ExportConfig.PropertyChanged -= ExportConfigOnPropertyChanged;
            models.Settings.PropertyChanged -= SettingsOnPropertyChanged;
            Crop.Dispose();
            models.Display.IsExporting = false;
        }

        private void ExportConfigOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ExportConfigModel.UseCropping):
                    OnPropertyChanged(nameof(UseCropping));
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportConfigModel.CropStart):
                case nameof(ExportConfigModel.CropEnd):
                    OnPropertyChanged(nameof(IsValid));
                    break;
                case nameof(ExportConfigModel.Layer):
                    if (models.ExportConfig.Layer < 0)
                        selectedLayer = AvailableLayers.Last();
                    else
                        selectedLayer = AvailableLayers[models.ExportConfig.Layer];
                    OnPropertyChanged(nameof(SelectedLayer));

                    // preview layer
                    if (models.ExportConfig.Layer >= 0)
                        models.Display.ActiveLayer = models.ExportConfig.Layer;
                    break;
                case nameof(ExportConfigModel.Mipmap):
                    if (models.ExportConfig.Mipmap < 0)
                        selectedMipmap = AvailableMipmaps.Last();
                    else
                        selectedMipmap = AvailableMipmaps[models.ExportConfig.Mipmap];
                    OnPropertyChanged(nameof(SelectedMipmap));
                    
                    Crop.Mipmap = selectedMipmap.Cargo;

                    // preview mipmap
                    models.Display.ActiveMipmap = Math.Max(models.ExportConfig.Mipmap, 0);
                    break;
            }
        }

        public bool IsValid => !UseCropping || (Crop.CropStartX <= Crop.CropEndX && Crop.CropStartY <= Crop.CropEndY);

        public string Filename
        {
            get => filename;
            set
            {
                // do nothing. the text box needs a read/write property but wont be changed anyways
            }
        }

        public List<ListItemViewModel<int>> AvailableLayers { get; } = new List<ListItemViewModel<int>>();
        public List<ListItemViewModel<int>> AvailableMipmaps { get; } = new List<ListItemViewModel<int>>();
        public List<ListItemViewModel<PixelDataType>> AvailableDataTypes { get; } = new List<ListItemViewModel<PixelDataType>>();
        public List<ListItemViewModel<GliFormat>> AvailableFormats { get; private set; }

        public bool EnableLayers => AvailableLayers.Count > 1;
        public bool EnableMipmaps => AvailableMipmaps.Count > 1;
        public bool EnableDataType => AvailableDataTypes.Count > 1;
        public bool EnableFormat => AvailableFormats.Count > 1;

        public Visibility ZCropVisibility => is3D ? Visibility.Visible : Visibility.Collapsed;

        private ListItemViewModel<int> selectedLayer;
        public ListItemViewModel<int> SelectedLayer
        {
            get => selectedLayer;
            set
            {
                if (value == null || value == selectedLayer) return;
                selectedLayer = value;
                models.ExportConfig.Layer = value.Cargo;
                OnPropertyChanged(nameof(SelectedLayer));
            }
        }

        private ListItemViewModel<int> selectedMipmap;
        public ListItemViewModel<int> SelectedMipmap
        {
            get => selectedMipmap;
            set
            {
                if (value == null || value == selectedMipmap) return;
                selectedMipmap = value;
                models.ExportConfig.Mipmap = value.Cargo;
                OnPropertyChanged(nameof(SelectedMipmap));
            }
        }

        private ListItemViewModel<PixelDataType> selectedDataType;

        public ListItemViewModel<PixelDataType> SelectedDataType
        {
            get => selectedDataType;
            set
            {
                if (value == null || value == selectedDataType) return;
                selectedDataType = value;

                AvailableFormats = new List<ListItemViewModel<GliFormat>>();
                selectedFormat = null;
                bool allowAll = selectedDataType.Cargo == PixelDataType.Undefined;
                int bestFit = 0;
                int bestFitValue = Int32.MinValue;
                for (var index = 0; index < allFormats.Count; index++)
                {
                    var formatBox = allFormats[index];
                    if (!allowAll && formatBox.Cargo.GetDataType() != selectedDataType.Cargo)
                        continue;

                    AvailableFormats.Add(formatBox);
                    // determine most appropriate format
                    if (formatRatings[index] > bestFitValue)
                    {
                        bestFitValue = formatRatings[index];
                        bestFit = index;
                    }
                }

                OnPropertyChanged(nameof(AvailableFormats));
                OnPropertyChanged(nameof(EnableFormat));
                SelectedFormat = allFormats[bestFit];
                
                OnPropertyChanged(nameof(SelectedDataType));
                
            }
        }

        #region ZOOMBOX

        public bool HasZoomBox => models.ZoomBox.Boxes.Count != 0;

        private static bool exportZoomBox = true;

        public bool ExportZoomBox
        {
            get => exportZoomBox;
            set
            {
                if(value == exportZoomBox) return;
                exportZoomBox = value;
                OnPropertyChanged(nameof(ExportZoomBox));
            }
        }

        public bool ZoomBorders
        {
            get => models.Settings.ExportZoomBoxBorder;
            set => models.Settings.ExportZoomBoxBorder = value;
        }

        public int ZoomBoxScale
        {
            get => models.Settings.ExportZoomBoxScale;
            set => models.Settings.ExportZoomBoxScale = value;
        }

        #endregion

        private ListItemViewModel<GliFormat> selectedFormat;
        public ListItemViewModel<GliFormat> SelectedFormat
        {
            get => selectedFormat;
            set
            {
                if (value == null || value == selectedFormat) return;
                selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Warning));
                HasQualityValue = usedFormat.SupportsQuality(value.Cargo);
            }
        }

        public GliFormat SelectedFormatValue => selectedFormat.Cargo;

        public string Description => GetExtendedDescription(selectedFormat.Cargo);

        private string GetExtendedDescription(GliFormat format)
        {
            var desc = format.GetDescription();

            if (extension == "ktx2" && !format.IsCompressed() && usedFormat.SupportsQuality(format)) // clarify about compression
            {
                string outputFormat = "UASTC";
                string compression = "Zstandard";
                if (format.GetDataType() == PixelDataType.Srgb)
                {
                    outputFormat = "ETC1S";
                    compression = "BasisLZ";
                }

                if (!String.IsNullOrEmpty(desc)) desc += ".\n";
                desc +=
                    $"Choosing a Quality below 100 will perform a block compression to {outputFormat} with an additional supercompression via {compression}.";
            }

            return desc;
        }

        public string Warning
        {
            get
            {
                var dataType = selectedFormat.Cargo.GetDataType();
                if (nonSrgbExportWarnings && dataType != PixelDataType.Srgb)
                {
                    var war = "Warning: This file format only supports sRGB formats. ";
                    if (dataType == PixelDataType.UNorm)
                        war += "Use SrgbAsUnorm(I0) inside image equation after importing.";
                    else if (dataType == PixelDataType.SNorm)
                        war += "Use SrgbAsSnorm(I0) inside image equation after importing.";

                    WarningVisibility = Visibility.Visible;
                    return war;
                }

                WarningVisibility = Visibility.Collapsed;
                return "";
            }
        }

        private Visibility warningVisibility = Visibility.Collapsed;

        public Visibility WarningVisibility
        {
            get => warningVisibility;
            set
            {
                if(value == warningVisibility) return;
                warningVisibility = value;
                OnPropertyChanged(nameof(WarningVisibility));
            }
        }

        #region CROPPONG

        public bool UseCropping
        {
            get => models.ExportConfig.UseCropping;
            set => models.ExportConfig.UseCropping = value;
        }

        public CropManager.ViewModel Crop { get; }

        #endregion

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
        public int MinQuality => ExportDescription.QualityMin;
        public int MaxQuality => ExportDescription.QualityMax;
        public int Quality
        {
            get => models.Settings.LastQuality;
            set => models.Settings.LastQuality = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
