using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageViewer.Models;
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
            this.decimalPlaces = models.Display.TexelDecimalPlaces;
            this.radius = models.Display.TexelRadius;
            this.alpha = models.Display.TexelDisplayAlpha;

            AvailableFormats.Add(new ComboBoxItem<DisplayModel.TexelDisplayMode>("decimal linear", DisplayModel.TexelDisplayMode.LinearDecimal));
            AvailableFormats.Add(new ComboBoxItem<DisplayModel.TexelDisplayMode>("decimal srgb", DisplayModel.TexelDisplayMode.SrgbDecimal));
            AvailableFormats.Add(new ComboBoxItem<DisplayModel.TexelDisplayMode>("byte linear", DisplayModel.TexelDisplayMode.LinearByte));
            AvailableFormats.Add(new ComboBoxItem<DisplayModel.TexelDisplayMode>("byte srgb", DisplayModel.TexelDisplayMode.SrgbByte));

            selectedFormat = AvailableFormats.Find(box => box.Cargo == models.Display.TexelDisplay);
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.TexelDecimalPlaces):
                    DecimalPlaces = models.Display.TexelDecimalPlaces;
                    break;
                case nameof(DisplayModel.TexelRadius):
                    Radius = models.Display.TexelRadius;
                    break;
                case nameof(DisplayModel.TexelDisplayAlpha):
                    Alpha = models.Display.TexelDisplayAlpha;
                    break;
                case nameof(DisplayModel.TexelDisplay):
                    SelectedFormat = AvailableFormats.Find(box => box.Cargo == models.Display.TexelDisplay);
                    break;
            }
        }

        public int MinDecimalPlaces => models.Display.MinTexelDecimalPlaces;
        public int MaxDecimalPlaces => models.Display.MaxTexelDecimalPlaces;
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

        private bool alpha;
        public bool Alpha
        {
            get => alpha;
            set
            {
                if (value == alpha) return;
                alpha = value;
                OnPropertyChanged(nameof(Alpha));
            }
        }

        public List<ComboBoxItem<DisplayModel.TexelDisplayMode>> AvailableFormats { get; } = new List<ComboBoxItem<DisplayModel.TexelDisplayMode>>();

        private ComboBoxItem<DisplayModel.TexelDisplayMode> selectedFormat;
        public ComboBoxItem<DisplayModel.TexelDisplayMode> SelectedFormat
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
            models.Display.TexelDisplay = SelectedFormat.Cargo;
            models.Display.TexelDecimalPlaces = DecimalPlaces;
            models.Display.TexelRadius = Radius;
            models.Display.TexelDisplayAlpha = Alpha;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
