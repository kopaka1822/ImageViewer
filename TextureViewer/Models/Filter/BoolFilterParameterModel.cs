using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models.Filter
{
    public class BoolFilterParameterModel : FilterParameterModel<bool>, IFilterParameter
    {
        public BoolFilterParameterModel(string name, int location, bool min, bool max, bool defaultValue) : base(name, location, min, max, defaultValue)
        {
            currentValue = defaultValue;
        }

        private bool currentValue;
        public override bool Value
        {
            get => currentValue;
            set
            {
                if (value == currentValue) return;

                currentValue = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        public ParameterType GetParamterType()
        {
            return ParameterType.Bool;
        }

        public FilterParameterModelBase GetBase()
        {
            return this;
        }

        public BoolFilterParameterModel GetBoolModel()
        {
            return this;
        }

        public IntFilterParameterModel GetIntModel()
        {
            throw new InvalidCastException();
        }

        public FloatFilterParameterModel GetFloatModel()
        {
            throw new InvalidCastException();
        }
    }
}
