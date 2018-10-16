using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.ViewModels.Filter;

namespace TextureViewer.Models.Filter
{
    public class BoolFilterParameterModel : FilterParameterModel<bool>, IFilterParameter
    {
        public class BoolParameterAction : ParameterAction
        {
            public BoolParameterAction(bool value, ModificationType modType) : base(value, modType)
            {
            }

            public override bool Invoke(bool value)
            {
                switch (ModType)
                {
                    case ModificationType.Add:
                        return value || OpValue;
                    case ModificationType.Multiply:
                        return value && OpValue;
                    case ModificationType.Set:
                        return OpValue;
                }

                return value;
            }
        }

        public BoolFilterParameterModel(string name, string variableName, bool min, bool max, bool defaultValue) : base(name, variableName, min, max, defaultValue)
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
