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
        /// separable filter that will be invoked twice with x- and y-direction (or thrice for 3D)
        /// </summary>
        public bool IsSepa { get; }

        /// <summary>
        /// repeast shader invocations until the filter kernel calls "abort_iterations"
        /// </summary>
        public bool DoIterations { get; }

        public int NumIterations
        {
            get
            {
                if (DoIterations) return int.MaxValue;
                if (!IsSepa) return 1;
                if (Target == FilterLoader.TargetType.Tex2D) return 2;
                return 3; // target 3d
            }
        }
        public FilterLoader.TargetType Target { get; }
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

        public int NumPipelines => isPipelineEnabled.Length;

        private bool[] isPipelineEnabled;

        /// <summary>
        /// will be called internally by Models
        /// </summary>
        internal FilterModel(FilterLoader loader, SharedModel shared, int numPipelines)
        {
            Parameters = loader.Parameters;
            TextureParameters = loader.TextureParameters;
            Target = loader.Target;
            IsSepa = loader.IsSepa;
            DoIterations = loader.Iterations;
            Name = loader.Name;
            Description = loader.Description;
            Filename = loader.Filename;
            isPipelineEnabled = new bool[numPipelines];
            for (var i = 0; i < numPipelines; ++i)
                isPipelineEnabled[i] = true;

            Shader = new FilterShader(this, loader.ShaderSource, loader.GroupSize, loader.KernelType, 
                loader.Target==FilterLoader.TargetType.Tex2D?ShaderBuilder.Builder2D:ShaderBuilder.Builder3D, shared);
        }

        // tries to create the same filter with a different target
        internal FilterModel Retarget(FilterLoader.TargetType newTarget, SharedModel shared)
        {
            var loader = new FilterLoader(Filename, newTarget);

            var res = new FilterModel(loader, shared, NumPipelines);

            // try to match parameters
            res.IsEnabled = IsEnabled;
            res.isPipelineEnabled = isPipelineEnabled;

            // int, float, bool parameters
            var numParams = Math.Min(res.Parameters.Count, Parameters.Count);
            for (int i = 0; i < numParams; ++i)
            {
                var psrc = Parameters[i];
                var pdst = res.Parameters[i];

                if (psrc.GetBase().Name == pdst.GetBase().Name &&
                    psrc.GetParamterType() == pdst.GetParamterType()) // probably the same parameter
                {
                    switch (psrc.GetParamterType())
                    {
                        case ParameterType.Float:
                            pdst.GetFloatModel().Value = psrc.GetFloatModel().Value;
                            break;
                        case ParameterType.Int:
                        case ParameterType.Enum:
                            pdst.GetIntModel().Value = psrc.GetIntModel().Value;
                            break;
                        case ParameterType.Bool:
                            pdst.GetBoolModel().Value = psrc.GetBoolModel().Value;
                            break;
                    }
                }
            }

            // no need to match texture parameters since the target type has changed

            return res;
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
