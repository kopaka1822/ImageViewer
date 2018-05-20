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
using TextureViewer.Utility.UI;

namespace TextureViewer.ViewModels.Dialog
{
    public class ExportViewModel : INotifyPropertyChanged
    {
        private readonly Models.Models models;
        private readonly ExportModel export;

        public ExportViewModel(Models.Models models, ExportModel export)
        {
            this.models = models;
            this.export = export;

            // init layers
            for (var i = 0; i < models.Images.NumLayers; ++i)
            {
                AvailableLayers.Add(new ComboBoxItem<int>("Layer " + i, i));
            }
            SelectedLayer = AvailableLayers[0];
            Debug.Assert(SelectedLayer.Cargo == export.Layer);

            // init mipmaps
            for (var i = 0; i < models.Images.NumMipmaps; ++i)
            {
                AvailableMipmaps.Add(new ComboBoxItem<int>("Mipmap " + i, i));
            }
            SelectedMipmap = AvailableMipmaps[0];
            Debug.Assert(SelectedMipmap.Cargo == export.Mipmap);

            // init formats
            foreach (var format in export.SupportedFormats)
            {
                AvailableFormat.Add(new ComboBoxItem<PixelFormat>(format.ToString().ToUpper(), format));
                if (format == export.PixelFormat)
                    SelectedFormat = AvailableFormat.Last();
            }
        }

        public string Filename
        {
            get => export.Filename;
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
                selectedLayer = value;
                export.Layer = selectedLayer.Cargo;
                OnPropertyChanged(nameof(SelectedLayer));
            }
        }

        private ComboBoxItem<int> selectedMipmap;
        public ComboBoxItem<int> SelectedMipmap
        {
            get => selectedMipmap;
            set
            {
                if (value == null || value == selectedMipmap) return;
                selectedMipmap = value;
                export.Mipmap = selectedMipmap.Cargo;
                OnPropertyChanged(nameof(SelectedMipmap));
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
                export.PixelFormat = selectedFormat.Cargo;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
