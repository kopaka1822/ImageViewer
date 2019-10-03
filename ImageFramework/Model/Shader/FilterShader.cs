using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter;

namespace ImageFramework.Model.Shader
{
    public class FilterShader : IDisposable
    {
        private readonly FilterModel parent;

        public FilterShader(FilterModel parent, string source)
        {
            this.parent = parent;
        }

        public void Dispose()
        {

        }
    }
}
