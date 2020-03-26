using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter;

namespace ImageViewer.Models.Settings
{
    public class FilterConfig
    {
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

        public List<Filter> Filters { get; } = new List<Filter>();

        public ViewerConfig.ImportMode ImportMode { get; set; }

        public static FilterConfig LoadFromModels(ModelsEx models)
        {
            var res = new FilterConfig();
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
            var filters = new List<FilterModel>();
            if (ImportMode == ViewerConfig.ImportMode.Add)
            {
                filters.AddRange(models.Filter.Filter);
            }

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

                filters.Add(model);
            }

            models.Filter.SetFilter(filters);
        }

        private static List<FilterParameter> GetFilterParameters(FilterModel filter)
        {
            var res = new List<FilterParameter>();
            // int, float, bool paramters
            foreach (var fp in filter.Parameters)
            {
                res.Add(new FilterParameter
                {
                    Name = fp.GetBase().VariableName,
                    Value = fp.GetBase().StringValue
                });
            }
            // texture parameters
            foreach (var tp in filter.TextureParameters)
            {
                res.Add(new FilterParameter
                {
                    Name = TexParamPrefix + tp.TextureName,
                    Value = tp.Source.ToString()
                });
            }

            return res;
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

        private static void SetFilterParameter(FilterModel model, string name, string value)
        {
            if (!name.StartsWith(TexParamPrefix))
            {
                foreach (var fp in model.Parameters)
                {
                    if (fp.GetBase().VariableName != name) continue;

                    fp.GetBase().StringValue = value;
                    return;
                }
            }
            else
            {
                var noPrefix = name.Substring(TexParamPrefix.Length);
                foreach (var tp in model.TextureParameters)
                {
                    if (tp.TextureName != noPrefix) continue;

                    tp.Source = int.Parse(value);
                    return;
                }
            }
        }

        private static readonly string TexParamPrefix = "__texture__";
    }
}
