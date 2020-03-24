using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter;
using ImageFramework.Model.Scaling;

namespace ImageViewer.Models.Settings
{
    public class EquationConfig
    {
        public class Equation
        {
            public bool IsEnabled { get; set; }
            public string ColorFormula { get; set; }
            public string AlphaFormula { get; set; }
            public bool GenMipmaps { get; set; }
            public bool UseFilter { get; set; }
        }

        public class FilterParameter
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

        public class Filter
        {
            public string Filename { get; set; }
            public bool IsEnabled { get; set; }

            public List<bool> IsPipelineEnabled { get; set; } = new List<bool>(4);

            public List<FilterParameter> Parameters { get; set; } = new List<FilterParameter>();
        }

        public List<Equation> Equations { get; } = new List<Equation>();
        public List<Filter> Filters { get; } = new List<Filter>();

        public ScalingModel.MinifyFilters MipmapTechnique { get; set; }

        public static EquationConfig LoadFromModels(ModelsEx models)
        {
            var res = new EquationConfig();
            // equation
            foreach (var pipe in models.Pipelines)
            {
                res.Equations.Add(new Equation
                {
                    IsEnabled = pipe.IsEnabled,
                    ColorFormula = pipe.Color.Formula,
                    AlphaFormula = pipe.Alpha.Formula,
                    GenMipmaps = pipe.RecomputeMipmaps,
                    UseFilter = pipe.UseFilter
                });
            }

            // mipmaps
            res.MipmapTechnique = models.Scaling.Minify;

            // filter
            foreach (var filter in models.Filter.Filter)
            {
                res.Filters.Add(new Filter
                {
                    Filename = filter.Filename,
                    IsEnabled = filter.IsEnabled,
                    IsPipelineEnabled = GetPipelineEnabled(models, filter),
                    Parameters = GetFilterParameters(filter),
                });
            }

            return res;
        }

        public void ApplyToModels(ModelsEx models)
        {
            if(models.Pipelines.Count != Equations.Count)
                throw new Exception("equation count mismatch");

            // equations
            for (var i = 0; i < Equations.Count; i++)
            {
                var equation = Equations[i];
                var pipe = models.Pipelines[i];
                pipe.IsEnabled = equation.IsEnabled;
                pipe.Color.Formula = equation.ColorFormula;
                pipe.Alpha.Formula = equation.AlphaFormula;
                pipe.RecomputeMipmaps = equation.GenMipmaps;
                pipe.UseFilter = equation.UseFilter;
            }

            // mipmaps
            models.Scaling.Minify = MipmapTechnique;

            // filter
            models.Filter.Clear();
            foreach (var filter in Filters)
            {
                var model = models.CreateFilter(filter.Filename);
                // set parameters
                model.IsEnabled = filter.IsEnabled;
                for (var i = 0; i < filter.IsPipelineEnabled.Count; i++)
                {
                    model.SetIsPipelineEnabled(i, filter.IsPipelineEnabled[i]);
                }

                foreach (var fp in filter.Parameters)
                {
                    SetFilterParameter(model, fp.Name, fp.Value);
                }

                models.Filter.AddFilter(model);
            }
        }

        private static List<bool> GetPipelineEnabled(ModelsEx models, FilterModel filter)
        {
            var res = new List<bool>(models.NumPipelines);
            for (int i = 0; i < models.NumPipelines; ++i)
            {
                res.Add(filter.IsPipelineEnabled(i));
            }

            return res;
        }

        private static List<FilterParameter> GetFilterParameters(FilterModel filter)
        {
            var res = new List<FilterParameter>();
            // int, float, bool paramters
            foreach (var fp in filter.Parameters)
            {
                res.Add(new FilterParameter
                {
                    Name = fp.GetBase().Name,
                    Value = fp.GetBase().StringValue
                });
            }
            // texture parameters
            foreach (var tp in filter.TextureParameters)
            {
                res.Add(new FilterParameter
                {
                    Name = tp.Name,
                    Value = tp.Source.ToString()
                });
            }

            return res;
        }

        private static void SetFilterParameter(FilterModel model, string name, string value)
        {
            foreach (var fp in model.Parameters)
            {
                if(fp.GetBase().Name != name) continue;

                fp.GetBase().StringValue = value;
                return;
            }

            foreach (var tp in model.TextureParameters)
            {
                if(tp.Name != name) continue;

                tp.Source = int.Parse(value);
            }
        }
    }
}
