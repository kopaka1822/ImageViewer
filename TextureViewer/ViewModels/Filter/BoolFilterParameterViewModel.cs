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
    public class BoolFilterParameterViewModel : FilterParameterViewModelBase<bool>
    {
        public BoolFilterParameterViewModel(BoolFilterParameterModel parameter)
        : base(parameter)
        {
            currentValue = parameter.Value;
        }

        private bool currentValue;
        public override bool Value
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
    }
}
