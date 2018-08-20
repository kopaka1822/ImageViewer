using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using TextureViewer.Annotations;
using TextureViewer.Models.Filter;

namespace TextureViewer.ViewModels.Filter
{
    public class FloatFilterParameterViewModel : FilterParameterViewModelBase<float>
    {
        private readonly FloatFilterParameterModel parameter;

        public FloatFilterParameterViewModel(FloatFilterParameterModel parameter)
        : base(parameter)
        {
            this.parameter = parameter;
            currentValue = parameter.Value;
        }

        private float currentValue;
        public override float Value
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
    }
}
