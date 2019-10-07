using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter.Parameter;

namespace ImageViewer.ViewModels.Filter
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

                if (prevChanged != HasChanges())
                    OnChanged();
            }
        }
    }
}
