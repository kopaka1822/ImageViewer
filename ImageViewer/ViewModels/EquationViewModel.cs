using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class EquationViewModel : INotifyPropertyChanged
    {
        private readonly ImagePipeline model;
        private readonly StatisticModel statistics;
        private readonly ModelsEx models;
        private readonly int imageId;

        public EquationViewModel(ModelsEx models, int imageId)
        {
            this.model = models.Pipelines[imageId];
            this.statistics = models.Statistics[imageId];
            this.models = models;
            this.imageId = imageId;
            this.Color = new FormulaViewModel(model.Color, models.Images, this);
            this.Alpha = new FormulaViewModel(model.Alpha, models.Images, this);
            Color.PropertyChanged += FormulaOnPropertyChanged;
            Alpha.PropertyChanged += FormulaOnPropertyChanged;

            this.useFilter = model.UseFilter;

            this.model.PropertyChanged += ModelOnPropertyChanged;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.Settings.PropertyChanged += SettingsOnPropertyChanged;
            this.statistics.PropertyChanged += StatisticsOnPropertyChanged;
        }

        private void StatisticsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StatisticModel.Stats):
                    OnPropertyChanged(nameof(TexelColor));
                    break;
            }
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.TexelDecimalPlaces):
                case nameof(SettingsModel.TexelDisplay):
                    OnPropertyChanged(nameof(TexelColor));
                    break;
            }
        }

        private void FormulaOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FormulaViewModel.HasChanges):
                    if (HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
                    break;
            }
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.TexelPosition):
                case nameof(DisplayModel.TexelRadius):
                    RecomputeTexelColor();
                    break;
            }
        }

        /// <summary>
        /// tries to apply the formulas in the text boxes.
        /// throws an exception on failure
        /// </summary>
        public void ApplyFormulas()
        {
            Color.Apply();
            Alpha.Apply();
            model.UseFilter = useFilter;
            prevHasChanged = false;
        }

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagePipeline.Image):
                    RecomputeTexelColor();
                    break;
                case nameof(ImagePipeline.IsEnabled):
                    OnPropertyChanged(nameof(IsVisible));
                    OnPropertyChanged(nameof(Visibility));
                    if (model.IsEnabled)
                        RecomputeTexelColor();
                    return;
                case nameof(ImagePipeline.UseFilter):
                    UseFilter = model.UseFilter;
                    return;
            }
        }

        public bool IsVisible
        {
            get => model.IsEnabled;
            set
            {
                model.IsEnabled = value;
                if (value) return;
                UseFilter = model.UseFilter;
            }
        }

        public Visibility Visibility => model.IsEnabled ? Visibility.Visible : Visibility.Collapsed;

        private bool useFilter;
        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
                if (HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
            }
        }

        public FormulaViewModel Color { get; }

        public FormulaViewModel Alpha { get; }

        private Color texelColor = ImageFramework.Utility.Color.Black;
        public string TexelColor
        {
            get
            {
                var c = texelColor;
                if (models.Settings.TexelDisplay == SettingsModel.TexelDisplayMode.SrgbByte ||
                    models.Settings.TexelDisplay == SettingsModel.TexelDisplayMode.SrgbDecimal)
                    c = c.ToSrgb();

                string res;

                if (models.Settings.TexelDisplay == SettingsModel.TexelDisplayMode.SrgbDecimal ||
                    models.Settings.TexelDisplay == SettingsModel.TexelDisplayMode.LinearDecimal)
                    res = c.ToDecimalString(statistics.Stats.HasAlpha, models.Settings.TexelDecimalPlaces);
                else if (models.Settings.TexelDisplay == SettingsModel.TexelDisplayMode.LinearFloat)
                    res = c.ToFloatString(statistics.Stats.HasAlpha, models.Settings.TexelDecimalPlaces);
                else // byte 
                    res = c.ToBitString(statistics.Stats.HasAlpha);

                return $"E{imageId + 1}: " + res;
            }
        }

        private void RecomputeTexelColor()
        {
            if (!model.IsEnabled) return;

            // check if the texture was already computed
            var texture = model.Image;
            if (texture == null) return;

            var color = models.GetPixelValue(texture, models.Display.TexelPosition.X, models.Display.TexelPosition.Y,
                models.Display.ActiveLayer, models.Display.ActiveMipmap, models.Display.TexelRadius);

            texelColor = color;
            OnPropertyChanged(nameof(TexelColor));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool prevHasChanged = false;
        public bool HasChanges => Color.HasChanges ||
                                  Alpha.HasChanges ||
                                  useFilter != model.UseFilter;

        /// <summary>
        /// indicates if the has changed property changed since the last query
        /// </summary>
        /// <returns></returns>
        private bool HasChangedChanged()
        {
            var changes = HasChanges;
            var res = prevHasChanged != changes;
            prevHasChanged = changes;
            return res;
        }
    }
}
