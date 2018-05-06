using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public EquationViewModel(ImageEquationModel model, Models.Models models)
        {
            this.model = model;
            this.models = models;
            this.colorFormula = model.ColorFormula.Formula;
            this.alphaFormula = model.AlphaFormula.Formula;
            this.useFilter = model.UseFilter;

            this.model.PropertyChanged += ModelOnPropertyChanged;
            this.model.ColorFormula.PropertyChanged += ColorFormulaOnPropertyChanged;
            this.model.AlphaFormula.PropertyChanged += AlphaFormulaOnPropertyChanged;
        }

        /// <summary>
        /// tries to apply the formulas in the text boxes.
        /// throws an exception on failure
        /// </summary>
        public void ApplyFormulas()
        {
            model.ColorFormula.Formula = colorFormula;
            model.AlphaFormula.Formula = alphaFormula;
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
                    return;
                case nameof(ImageEquationModel.UseFilter):
                    UseFilter = model.UseFilter;
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
