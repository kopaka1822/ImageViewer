using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Commands;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Display;

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
            this.recomputeMipmaps = model.RecomputeMipmaps;

            this.model.PropertyChanged += ModelOnPropertyChanged;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.Settings.PropertyChanged += SettingsOnPropertyChanged;
            this.statistics.PropertyChanged += StatisticsOnPropertyChanged;

            ToggleAlphaCommand = new ActionCommand(() => AutoAlpha = !AutoAlpha);
            ToggleVisibilityCommand = new ActionCommand(() => IsVisible = !IsVisible);

            models.PipelineUpdate += OnPipelineUpdate;

            AdjustAlphaFormula();
        }

        private void OnPipelineUpdate(object sender, bool success)
        {
            //if (success) return;

            // some pipelines could not be executed => in this case the image is not set
            if (model.IsEnabled && model.IsValid && !HasChanges && model.Image == null)
            {
                applicationFailed = true;
                if(HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
            }
            // if the pipeline has previously failed, but now succeded, remove the application failed flag
            if(applicationFailed && model.Image != null)
            {
                applicationFailed = false;
                if (HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
            }
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
                case nameof(FormulaViewModel.FirstImageId):
                    AdjustAlphaFormula();
                    break;
            }
        }

        private void DisplayOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DisplayModel.TexelPosition):
                case nameof(DisplayModel.TexelRadius):
                case nameof(DisplayModel.ActiveLayer):
                case nameof(DisplayModel.ActiveMipmap):
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
            model.RecomputeMipmaps = recomputeMipmaps;
            if (HasChangedChanged())
                OnPropertyChanged(nameof(HasChanges));
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
                    OnPropertyChanged(nameof(UseAlphaEquation));
                    if (model.IsEnabled)
                        RecomputeTexelColor();
                    return;
                case nameof(ImagePipeline.UseFilter):
                    UseFilter = model.UseFilter;
                    return;
                case nameof(ImagePipeline.RecomputeMipmaps):
                    RecomputeMipmaps = model.RecomputeMipmaps;
                    break;
            }
        }

        public string Title => $"Equation {imageId + 1}";

        public int Id => imageId;
        public bool IsVisible
        {
            get => model.IsEnabled;
            set
            {
                model.IsEnabled = value;
                //if (value) return;
                UseFilter = model.UseFilter;
                RecomputeMipmaps = model.RecomputeMipmaps;
                // this might have changed while invisible
                if(value)
                    OnPropertyChanged(nameof(HasChanges));
            }
        }

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

        private bool recomputeMipmaps;

        public bool RecomputeMipmaps
        {
            get => recomputeMipmaps;
            set
            {
                if (value == recomputeMipmaps) return;
                recomputeMipmaps = value;
                OnPropertyChanged(nameof(RecomputeMipmaps));
                if(HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
            }
        }

        public bool UseAlphaEquation => IsVisible && !AutoAlpha;

        public ICommand ToggleAlphaCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }

        private bool autoAlpha = true;
        public bool AutoAlpha
        {
            get => autoAlpha;
            set
            {
                if (value == autoAlpha) return;
                autoAlpha = value;
                OnPropertyChanged(nameof(AutoAlpha));
                OnPropertyChanged(nameof(UseAlphaEquation));

                if (AutoAlpha)
                {
                    AdjustAlphaFormula();
                }
            }
        }

        public void AdjustAlphaFormula()
        {
            if (!AutoAlpha) return;
            // change in view model
            Alpha.Formula = $"I{Color.FirstImageId}";
        }

        public FormulaViewModel Color { get; }

        public FormulaViewModel Alpha { get; }

        private Color texelColor = ImageFramework.Utility.Colors.Black;
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
                    res = c.ToDecimalString(statistics.Stats.IsGrayscale, statistics.Stats.HasAlpha, models.Settings.TexelDecimalPlaces);
                else if (models.Settings.TexelDisplay == SettingsModel.TexelDisplayMode.LinearFloat)
                    res = c.ToFloatString(statistics.Stats.IsGrayscale, statistics.Stats.HasAlpha, models.Settings.TexelDecimalPlaces);
                else // byte 
                    res = c.ToBitString(statistics.Stats.IsGrayscale, statistics.Stats.HasAlpha);

                return $"E{imageId + 1}: " + res;
            }
        }

        private void RecomputeTexelColor()
        {
            if (!model.IsEnabled) return;

            // check if the texture was already computed
            var texture = model.Image;
            if (texture == null) return;

            if (models.Display.TexelPosition.HasValue)
            {
                var color = models.GetPixelValue(texture, models.Display.TexelPosition.Value,
                    models.Display.ActiveLayerMipmap, models.Display.TexelRadius);

                texelColor = color;
            }
            else
            {
                texelColor = Colors.Transparent;
            }

            OnPropertyChanged(nameof(TexelColor));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // indicates if the formula was applied, but some error occured during execution
        private bool applicationFailed = false;

        private bool prevHasChanged = false;
        public bool HasChanges => Color.HasChanges ||
                                  Alpha.HasChanges ||
                                  useFilter != model.UseFilter ||
                                  recomputeMipmaps != model.RecomputeMipmaps ||
                                  applicationFailed;

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
