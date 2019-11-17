using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Views;

namespace ImageViewer.ViewModels.Dialog
{
    public class PixelDisplayViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public PixelDisplayViewModel(ModelsEx models)
        {
            this.models = models;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.Settings.PropertyChanged += SettingsOnPropertyChanged;
            this.decimalPlaces = models.Settings.TexelDecimalPlaces;
            this.radius = models.Display.TexelRadius;

            AvailableFormats.Add(new ComboBoxItem<SettingsModel.TexelDisplayMode>("decimal (linear)", SettingsModel.TexelDisplayMode.LinearDecimal));
            AvailableFormats.Add(new ComboBoxItem<SettingsModel.TexelDisplayMode>("float (linear)", SettingsModel.TexelDisplayMode.LinearFloat));
            AvailableFormats.Add(new ComboBoxItem<SettingsModel.TexelDisplayMode>("decimal (sRGB)", SettingsModel.TexelDisplayMode.SrgbDecimal));
            AvailableFormats.Add(new ComboBoxItem<SettingsModel.TexelDisplayMode>("byte (sRGB)", SettingsModel.TexelDisplayMode.SrgbByte));

            selectedFormat = AvailableFormats.Find(box => box.Cargo == models.Settings.TexelDisplay);
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.TexelDecimalPlaces):
                    DecimalPlaces = models.Settings.TexelDecimalPlaces;
                    break;
                case nameof(SettingsModel.TexelDisplay):
                    SelectedFormat = AvailableFormats.Find(box => box.Cargo == models.Settings.TexelDisplay);
                    break;
            }
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.TexelRadius):
                    Radius = models.Display.TexelRadius;
                    break;
            }
        }

        public int MinDecimalPlaces => models.Settings.MinTexelDecimalPlaces;
        public int MaxDecimalPlaces => models.Settings.MaxTexelDecimalPlaces;
        public int MinRadius => models.Display.MinTexelRadius;
        public int MaxRadius => models.Display.MaxTexelRadius;

        private int decimalPlaces;
        public int DecimalPlaces
        {
            get => decimalPlaces;
            set
            {
                if (value == decimalPlaces) return;
                decimalPlaces = value;
                OnPropertyChanged(nameof(DecimalPlaces));
            }
        }

        private int radius;
        public int Radius
        {
            get => radius;
            set
            {
                if (value == radius) return;
                radius = value;
                OnPropertyChanged(nameof(Radius));
            }
        }

        public List<ComboBoxItem<SettingsModel.TexelDisplayMode>> AvailableFormats { get; } = new List<ComboBoxItem<SettingsModel.TexelDisplayMode>>();

        private ComboBoxItem<SettingsModel.TexelDisplayMode> selectedFormat;
        public ComboBoxItem<SettingsModel.TexelDisplayMode> SelectedFormat
        {
            get => selectedFormat;
            set
            {
                if (ReferenceEquals(value, selectedFormat)) return;
                selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        public void Unregister()
        {
            this.models.Display.PropertyChanged -= DisplayOnPropertyChanged;
        }

        public void Apply()
        {
            models.Settings.TexelDisplay = SelectedFormat.Cargo;
            models.Settings.TexelDecimalPlaces = DecimalPlaces;
            models.Display.TexelRadius = Radius;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
