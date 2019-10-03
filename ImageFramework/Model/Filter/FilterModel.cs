using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter.Parameter;

namespace ImageFramework.Model.Filter
{
    public class FilterModel
    {
        public IReadOnlyList<IFilterParameter> Parameters { get; }

        public IReadOnlyList<TextureFilterParameterModel> TextureParameters { get; }

        /// <summary>
        /// separable filter that will be invoked twice with x- and y-direction
        /// </summary>
        public bool IsSepa { get; }

        public string Name { get; }

        public string Description { get; }

        public string Filename { get; }

        public bool IsEnabled { get; set; } = true;

        public bool IsPipelineEnabled { get; set; }
    }
}
