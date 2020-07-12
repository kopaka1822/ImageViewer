using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using Newtonsoft.Json;

namespace ImageViewer.Models.Settings
{
    public class ViewerConfig
    {
        [Flags]
        public enum Components
        {
            None,
            Images = 1 << 0,
            Equations = 1 << 1,
            Display = 1 << 2,
            Export = 1 << 3,
            Filter = 1 << 4,
            All = 0xFFFFFFF
        }

        public enum ImportMode
        {
            Replace, // replace with previous
            Add // add to previous
        }

        public static ViewerConfig LoadFromFile(string filename)
        {
            var txt = File.ReadAllText(filename);
            var res = JsonConvert.DeserializeObject<ViewerConfig>(txt);
            if(!res.Version.HasValue)
                throw new Exception("version property missing");

            if(res.Version.Value > CurrentVersion)
                throw new Exception("config version not supported (too high)");
            if(res.Version.Value < 5 && res.Images != null)
                throw new Exception("images in current config version not supported");

            return res;
        }

        public void WriteToFile(string filename)
        {
            Version = CurrentVersion;
            var txt = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filename, txt);
        }

        public async Task ApplyToModels(ModelsEx models)
        {
            if (Images != null)
            {
                await Images.ApplyToModels(models);
            }

            Equation?.ApplyToModels(models);
            Filter?.ApplyToModels(models);
            Display?.ApplyToModels(models);
            Export?.ApplyToModels(models);
        }

        public static ViewerConfig LoadFromModels(ModelsEx models, Components c)
        {
            var res = new ViewerConfig();
            if (c.HasFlag(Components.Images))
            {
                res.Images = ImagesConfig.LoadFromModels(models);
            }

            if (c.HasFlag(Components.Equations))
            {
                res.Equation = EquationConfig.LoadFromModels(models);
            }

            if (c.HasFlag(Components.Filter))
            {
                res.Filter = FilterConfig.LoadFromModels(models);
            }

            if (c.HasFlag(Components.Display))
            {
                res.Display = DisplayConfig.LoadFromModels(models);
            }

            if (c.HasFlag(Components.Export))
            {
                res.Export = ExportConfig.LoadFromModels(models);
            }

            return res;
        }

        public int? Version { get; set; }
        public ImagesConfig Images { get; set; }
        public EquationConfig Equation { get; set; }
        public FilterConfig Filter { get; set; }
        public DisplayConfig Display { get; set; }

        public ExportConfig Export { get; set; }

        private static readonly int CurrentVersion = 5;

    }
}
