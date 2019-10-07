using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter.Parameter;

namespace ImageViewer.ViewModels.Filter
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
