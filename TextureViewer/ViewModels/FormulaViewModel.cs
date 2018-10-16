using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.Models;

namespace TextureViewer.ViewModels
{
    public class FormulaViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly FormulaModel model;
        private readonly EquationViewModel parent;

        public FormulaViewModel(FormulaModel model, ImagesModel images, EquationViewModel parent)
        {
            this.model = model;
            this.formula = model.Formula;
            this.parent = parent;

            model.PropertyChanged += ModelOnPropertyChanged;
            images.PropertyChanged += ImagesOnPropertyChanged;
            parent.PropertyChanged += ParentOnPropertyChanged;
        }

        private void ParentOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch(args.PropertyName)
            {
                case nameof(EquationViewModel.IsVisible):
                    // redo validation (only display invalid on visible formulas)
                    OnPropertyChanged(nameof(Formula));
                    break;
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    // redo validation (formula might become invalid or valid after number of images changes)
                    OnPropertyChanged(nameof(Formula));
                    break;
            }
        }

        private string formula;
        public string Formula
        {
            get => formula;
            set
            {
                if (value == null || value == formula) return;
                var prevChanges = HasChanges;
                formula = value;
                OnPropertyChanged(nameof(Formula));
                if(prevChanges != HasChanges)
                    OnPropertyChanged(nameof(HasChanges));
            }
        }

        public bool IsVisible => parent.IsVisible;

        public bool HasChanges => Formula != model.Formula;

        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(FormulaModel.Formula)) return;
            // this property probably changed
            OnPropertyChanged(nameof(HasChanges));
        }

        /// <summary>
        /// tries to apply the formula
        /// throws an exception on failure
        /// </summary>
        public void Apply()
        {
            model.Formula = formula;
        }

        #region DataError

        /// <summary>
        /// Will be called for each and every property when ever its value is changed
        /// </summary>
        /// <param name="columnName">Name of the property whose value is changed</param>
        /// <returns></returns>
        public string this[string columnName] => Validate(columnName);

        private string Validate(string property)
        {
            if (property != nameof(Formula)) return string.Empty;

            // only display error if formula is visible
            if (!IsVisible) return string.Empty;

            var error = model.TestFormula(Formula);
            if (error == null) return string.Empty;
            return error;
        }

        public string Error => String.Empty;

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
