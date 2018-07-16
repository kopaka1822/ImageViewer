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
            this.colorFormula = model.ColorFormula.Formula;
            this.alphaFormula = model.AlphaFormula.Formula;
            this.useFilter = model.UseFilter;

            this.model.PropertyChanged += ModelOnPropertyChanged;
            this.model.ColorFormula.PropertyChanged += ColorFormulaOnPropertyChanged;
            this.model.AlphaFormula.PropertyChanged += AlphaFormulaOnPropertyChanged;
            this.models.Display.PropertyChanged += DisplayOnPropertyChanged;
            this.models.FinalImages.Get(imageId).PropertyChanged += FinalImageOnPropertyChanged;
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
            model.ColorFormula.Formula = colorFormula;
            model.AlphaFormula.Formula = alphaFormula;
            model.UseFilter = useFilter;
        }

        private void ColorFormulaOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FormulaModel.Formula):
                    ColorFormula = model.ColorFormula.Formula;
                    break;
            }
        }

        private void AlphaFormulaOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(FormulaModel.Formula):
                    AlphaFormula = model.AlphaFormula.Formula;
                    break;
            }
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
                // restore default values if tempory changes happend.
                // it would probably confuse the user otherwise if he
                // sees a formula that is not used on reenabling.
                AlphaFormula = model.AlphaFormula.Formula;
                ColorFormula = model.ColorFormula.Formula;
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
            }
        }

        private string colorFormula;
        public string ColorFormula
        {
            get => colorFormula;
            set
            {
                if (value == null || value.Equals(colorFormula)) return;
                colorFormula = value;
                OnPropertyChanged(nameof(ColorFormula));
            }
        }

        private string alphaFormula;
        public string AlphaFormula
        {
            get => alphaFormula;
            set
            {
                if (value == null || value.Equals(alphaFormula)) return;
                alphaFormula = value;
                OnPropertyChanged(nameof(AlphaFormula));
            }
        }

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

        public bool HasChanges()
        {
            return
                !alphaFormula.Equals(model.AlphaFormula.Formula) ||
                !colorFormula.Equals(model.ColorFormula.Formula) ||
                useFilter != model.UseFilter;

        }
    }
}
