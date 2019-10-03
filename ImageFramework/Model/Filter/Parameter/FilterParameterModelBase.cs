using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Filter.Parameter
{
    /// <summary>
    /// filter parameter information which is not dependent from the parameter type
    /// </summary>
    public class FilterParameterModelBase
    {
        public FilterParameterModelBase(string name, string variableName)
        {
            Name = name;
            VariableName = variableName;
        }

        // name for the filter menu
        public string Name { get; }
        // name within the shader
        public string VariableName { get; }
    }
}
