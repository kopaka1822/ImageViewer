using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Filter.Parameter;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model.Filter
{
    public class FilterModel : IDisposable, INotifyPropertyChanged
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

        private bool isEnabled = true;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value == isEnabled) return;
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private bool[] isPipelineEnabled;

        public FilterModel(FilterLoader loader, int numPipelines)
        {
            Parameters = loader.Parameters;
            TextureParameters = loader.TextureParameters;
            IsSepa = loader.IsSepa;
            Name = loader.Name;
            Description = loader.Description;
            Filename = loader.Filename;
            isPipelineEnabled = new bool[numPipelines];
            for (var i = 0; i < numPipelines; ++i)
                isPipelineEnabled[i] = true;

            Shader = new FilterShader(this, loader.ShaderSource);
        }

        public void SetIsPipelineEnabled(int index, bool value)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < isPipelineEnabled.Length);
            if (value == isPipelineEnabled[index]) return;
            isPipelineEnabled[index] = value;
            OnPropertyChanged(nameof(IsPipelineEnabled));
        }

        public bool IsPipelineEnabled(int index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < isPipelineEnabled.Length);
            return isPipelineEnabled[index];
        }

        /// <summary>
        /// returns true if this filter should be used by the specified pipeline
        /// </summary>
        public bool IsEnabledFor(int pipelineId)
        {
            Debug.Assert(pipelineId >= 0);
            Debug.Assert(pipelineId < isPipelineEnabled.Length);
            return IsEnabled && isPipelineEnabled[pipelineId];
        }

        public void Dispose()
        {
            Shader?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
