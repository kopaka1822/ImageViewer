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
    public class FormulaViewModel : INotifyPropertyChanged
    {
        private readonly FormulaModel model;

        public FormulaViewModel(FormulaModel model)
        {
            this.model = model;
            this.formula = model.Formula;

            model.PropertyChanged += ModelOnPropertyChanged;
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

            Formula = model.Formula;
        }

        /// <summary>
        /// tries to apply the formula
        /// throws an exception on failure
        /// </summary>
        public void Apply()
        {
            model.Formula = formula;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
