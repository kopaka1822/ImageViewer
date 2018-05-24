using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models.Filter
{
    public class FloatFilterParameterModel : FilterParameterModel<float>, IFilterParameter
    {
        public FloatFilterParameterModel(string name, int location, float min, float max, float defaultValue) 
            : base(name, location, min, max, defaultValue)
        {
            currentValue = defaultValue;
        }

        private float currentValue;
        public override float Value
        {
            get => currentValue;
            set
            {
                var clamped = Math.Min(Math.Max(value, Min), Max);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (currentValue == clamped) return;

                currentValue = clamped;
                OnPropertyChanged(nameof(Value));
            }
        }

        public ParameterType GetParamterType()
        {
            return ParameterType.Float;
        }

        public FilterParameterModelBase GetBase()
        {
            return this;
        }

        public BoolFilterParameterModel GetBoolModel()
        {
            throw new InvalidCastException();
        }

        public IntFilterParameterModel GetIntModel()
        {
            throw new InvalidCastException();
        }

        public FloatFilterParameterModel GetFloatModel()
        {
            return this;
        }
    }
}
