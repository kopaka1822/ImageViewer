using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using TextureViewer.Controller.Filter;
using TextureViewer.Controller.ImageCombination;
using TextureViewer.Models.Shader;
using TextureViewer.Utility;

namespace TextureViewer.Models.Filter
{
    public class FilterModel
    {
        public FilterShader Shader { get; }
        public List<IFilterParameter> Parameters { get; }
        public List<FilterTextureParameterModel> TextureParameters { get; }
        public bool IsSepa { get; }
        public bool IsSingleInvocation { get; }
        public string Name { get; }
        public string Description { get; }
        public string Filename { get; }
        public bool IsVisible { get; set; } = true;
        public bool[] IsEquationVisible { get; } = new bool[App.MaxImageViews];

        public FilterModel(FilterLoader loader)
        {
            Parameters = loader.Parameters;
            TextureParameters = loader.TextureParameters;
            IsSepa = loader.IsSepa;
            IsSingleInvocation = loader.IsSingleInvocation;
            Name = loader.Name;
            Description = loader.Description;
            Filename = loader.Filename;
            for (var i = 0; i < IsEquationVisible.Length; ++i)
                IsEquationVisible[i] = true;

            Shader = new FilterShader(loader.ShaderSource, IsSepa);
        }

        public void Dispose()
        {
            Shader.Dispose();
        }

        /// <summary>
        /// returns true if the filter should be applied for the equation
        /// </summary>
        /// <param name="equationId">equation index</param>
        /// <returns></returns>
        public bool IsVisibleFor(int equationId)
        {
            Debug.Assert(equationId >= 0);
            Debug.Assert(equationId < App.MaxImageViews);
            return IsEquationVisible[equationId] && IsVisible;
        }

        public IStepable MakeStepable(Models models, ImageCombineBuilder builder)
        {
            if (!IsSepa)
                return MakeIterationStepable(models, builder, 0);

            var steps = new List<IStepable>
            {
                MakeIterationStepable(models, builder, 0),
                MakeIterationStepable(models, builder, 1)
            };

            return new StepList(steps);
        }

        private IStepable MakeIterationStepable(Models models, ImageCombineBuilder builder, int iteration)
        {
            var steps = new List<IStepable>();
            for (int layer = 0; layer < models.Images.NumLayers; ++layer)
            {
                for (int mipmap = 0; mipmap < models.Images.NumMipmaps; ++mipmap)
                {
                    if (IsSingleInvocation)
                    {
                        steps.Add(new SingleDispatchStepper(models, this, builder, layer: layer, mipmap: mipmap, iteration: iteration));
                    }
                    else
                    {
                        steps.Add(new MultiDispatchStepper(models, this, builder, layer: layer, mipmap: mipmap, iteration: iteration));
                    }
                }
            }
            return new FilterStepList(builder, steps);
        }
    }
}
