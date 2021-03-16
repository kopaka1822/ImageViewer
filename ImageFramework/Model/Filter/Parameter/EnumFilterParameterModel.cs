using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Filter.Parameter
{
    public class EnumFilterParameterModel : IntFilterParameterModel
    {
        public EnumFilterParameterModel(string name, string variableName, List<string> enumValues, int defaultValue) 
            : base(name, variableName, 0, enumValues.Count - 1, defaultValue)
        {
            this.EnumValues = enumValues;
        }

        public List<string> EnumValues { get; }

        public string DisplayValue => EnumValues[Value];

        public override ParameterType GetParamterType()
        {
            return ParameterType.Enum;
        }
    }
}
