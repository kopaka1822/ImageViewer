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

        public FormulaViewModel(FormulaModel model, ImagesModel images)
        {
            this.model = model;
            this.formula = model.Formula;

            model.PropertyChanged += ModelOnPropertyChanged;
            images.PropertyChanged += ImagesOnPropertyChanged;
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
