using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class EquationViewModel : INotifyPropertyChanged
    {
        private readonly ImageEquationModel model;
        private readonly Models.Models models;
        private readonly int imageId;

        public EquationViewModel(ImageEquationModel model, Models.Models models, int imageId)
        {
            this.model = model;
            this.models = models;
            this.imageId = imageId;
            this.Color = new FormulaViewModel(model.ColorFormula, models.Images);
            this.Alpha = new FormulaViewModel(model.AlphaFormula, models.Images);
            Color.PropertyChanged += FormulaOnPropertyChanged;
            Alpha.PropertyChanged += FormulaOnPropertyChanged;

            this.useFilter = model.UseFilter;

            this.model.PropertyChanged += ModelOnPropertyChanged;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.FinalImages.Get(imageId).PropertyChanged += FinalImageOnPropertyChanged;
        }

        private void FormulaOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FormulaViewModel.HasChanges):
                    if(HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
                    break;
            }
        }

        private void FinalImageOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FinalImageModel.StatisticsTexture):
                    RecomputeTexelColor();
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
                case nameof(DisplayModel.TexelDisplay):
                case nameof(DisplayModel.TexelDisplayAlpha):
                case nameof(DisplayModel.TexelDecimalPlaces):
                    OnPropertyChanged(nameof(TexelColor));
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
                case nameof(ImageEquationModel.Visible):
                    OnPropertyChanged(nameof(IsVisible));
                    OnPropertyChanged(nameof(Visibility));
                    if(model.Visible)
                        RecomputeTexelColor();
                    return;
                case nameof(ImageEquationModel.UseFilter):
                    UseFilter = model.UseFilter;
                    return;
                case nameof(ImageEquationModel.TexelColor):
                    OnPropertyChanged(nameof(TexelColor));
                    return;
            }
        }

        public bool IsVisible
        {
            get => model.Visible;
            set
            {
                model.Visible = value;
                if (value) return;
                UseFilter = model.UseFilter;
            }
        }

        public Visibility Visibility => model.Visible ? Visibility.Visible : Visibility.Collapsed;

        private bool useFilter;
        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
                if(HasChangedChanged()) OnPropertyChanged(nameof(HasChanges));
            }
        }

        public FormulaViewModel Color { get; }

        public FormulaViewModel Alpha { get; }

        public string TexelColor
        {
            get
            {
                var c = model.TexelColor;
                if (models.Display.TexelDisplay == DisplayModel.TexelDisplayMode.SrgbByte ||
                    models.Display.TexelDisplay == DisplayModel.TexelDisplayMode.SrgbDecimal)
                    c = c.ToSrgb();

                string res;

                if (models.Display.TexelDisplay == DisplayModel.TexelDisplayMode.SrgbDecimal ||
                    models.Display.TexelDisplay == DisplayModel.TexelDisplayMode.LinearDecimal)
                    res = c.ToDecimalString(models.Display.TexelDisplayAlpha, models.Display.TexelDecimalPlaces);
                else
                    res = c.ToBitString(models.Display.TexelDisplayAlpha);

                return $"E{imageId + 1}: " + res;
            }
        }


        // => model.TexelColor.ToDecimalString(true, 3);

        private void RecomputeTexelColor()
        {
            if (!model.Visible) return;
            // check if the texture was already computed
            var texture = models.FinalImages.Get(imageId).StatisticsTexture;
            if (texture == null) return;

            var shader = models.GlData.GetPixelShader;

            var disableGl = models.GlContext.Enable();
            try
            {
                models.GlData.BindSampler(shader.GetTextureLocation(), true, false);
                texture.BindAsTexture2D(shader.GetTextureLocation(), models.Display.ActiveLayer,
                    models.Display.ActiveMipmap);

                models.Equations.Get(imageId).TexelColor =
                    shader.GetPixelColor(models.Display.TexelPosition.X, models.Display.TexelPosition.Y, models.Display.TexelRadius);
            }
            catch (Exception e)
            {
                App.ShowErrorDialog(models.App.Window, e.Message);
            }
            finally
            {
                if(disableGl)
                    models.GlContext.Disable();
            }
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
