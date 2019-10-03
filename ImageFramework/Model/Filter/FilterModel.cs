using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter.Parameter;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model.Filter
{
    public class FilterModel : IDisposable
    {
        internal FilterShader Shader { get; }

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

        public bool[] IsPipelineEnabled { get; set; }

        public FilterModel(FilterLoader loader, int numPipelines)
        {
            Parameters = loader.Parameters;
            TextureParameters = loader.TextureParameters;
            IsSepa = loader.IsSepa;
            Name = loader.Name;
            Description = loader.Description;
            Filename = loader.Filename;
            IsPipelineEnabled = new bool[numPipelines];
            for (var i = 0; i < numPipelines; ++i)
                IsPipelineEnabled[i] = true;

            Shader = new FilterShader(this, loader.ShaderSource);
        }

        /// <summary>
        /// returns true if this filter should be used by the specified pipeline
        /// </summary>
        public bool IsEnabledFor(int pipelineId)
        {
            Debug.Assert(pipelineId >= 0);
            Debug.Assert(pipelineId < IsPipelineEnabled.Length);
            return IsEnabled && IsPipelineEnabled[pipelineId];
        }

        public void Dispose()
        {
            Shader?.Dispose();
        }
    }
}
