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
    public class IntFilterParameterViewModel : FilterParameterViewModelBase<int>
    {
        private readonly IntFilterParameterModel parameter;

        public IntFilterParameterViewModel(IntFilterParameterModel parameter)
        : base(parameter)
        {
            this.parameter = parameter;
            currentValue = parameter.Value;
        }

        private int currentValue;
        public override int Value
        {
            get => currentValue;
            set
            {
                var prevChanges = HasChanges();

                var clamped = Math.Min(Math.Max(value, parameter.Min), parameter.Max);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (currentValue == clamped) return;
                currentValue = clamped;
                OnPropertyChanged(nameof(Value));
                
                if(prevChanges != HasChanges())
                    OnChanged();
            }
        }
    }
}
