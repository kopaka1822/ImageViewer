using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models.Filter
{
    /// <summary>
    /// filter parameter information which is not dependent from the parameter type
    /// </summary>
    public class FilterParameterModelBase
    {
        public FilterParameterModelBase(string name, int location)
        {
            Name = name;
            Location = location;
        }

        public string Name { get; }
        public int Location { get; }
    }
}
