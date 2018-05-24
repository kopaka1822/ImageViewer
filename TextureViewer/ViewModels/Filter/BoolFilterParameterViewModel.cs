using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.Models.Filter;

namespace TextureViewer.ViewModels.Filter
{
    public class BoolFilterParameterViewModel : INotifyPropertyChanged, IFilterParameterViewModel
    {
        private readonly BoolFilterParameterModel parameter;

        public BoolFilterParameterViewModel(BoolFilterParameterModel parameter)
        {
            this.parameter = parameter;
            parameter.PropertyChanged += ParameterOnPropertyChanged;
            currentValue = parameter.Value;
        }

        private void ParameterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(BoolFilterParameterModel.Value))
            {

            }
        }

        public void Apply()
        {
            parameter.Value = Value;
        }

        public void Cancel()
        {
            Value = parameter.Value;
        }

        public void RestoreDefaults()
        {
            Value = parameter.Default;
        }

        public bool HasChanges()
        {
            return Value != parameter.Value;
        }

        public event EventHandler Changed;

        private bool currentValue;
        public bool Value
        {
            get => currentValue;
            set
            {
                if (currentValue == value) return;
                currentValue = value;
                OnPropertyChanged(nameof(Value));
                OnChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
