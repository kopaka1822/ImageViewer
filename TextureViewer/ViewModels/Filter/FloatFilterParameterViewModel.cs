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
    public class FloatFilterParameterViewModel : INotifyPropertyChanged, IFilterParameterViewModel
    {
        private readonly FloatFilterParameterModel parameter;

        public FloatFilterParameterViewModel(FloatFilterParameterModel parameter)
        {
            this.parameter = parameter;
            this.parameter.PropertyChanged += ParameterOnPropertyChanged;
            currentValue = parameter.Value;
        }

        private void ParameterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(FloatFilterParameterModel.Value))
            {
                Value = parameter.Value;
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

        private float currentValue;
        public float Value
        {
            get => currentValue;
            set
            {
                var prevChanged = HasChanges();

                var clamped = Math.Min(Math.Max(value, parameter.Min), parameter.Max);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (currentValue == clamped) return;
                currentValue = clamped;
                OnPropertyChanged(nameof(Value));

                if(prevChanged != HasChanges())
                    OnChanged();
            }
        }

        public bool HasChanges()
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return Value != parameter.Value;
        }

        public event EventHandler Changed;

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
