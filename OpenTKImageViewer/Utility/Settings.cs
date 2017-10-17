using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenTKImageViewer.Utility
{
    public class Settings
    {
        public class Config
        {
            public Config()
            {
                ImagePath = "";
                TonemapperPath = "";
                ExportPath = "";
            }

            public string ImagePath;
            public string TonemapperPath;
            public string ExportPath;
        }

        private readonly Config config = null;
        private readonly string filename;

        public Settings(string filename)
        {
            this.filename = filename;
            try
            {
                using (var r = File.OpenText(filename))
                {
                    config = JsonConvert.DeserializeObject<Config>(r.ReadToEnd());
                }
            }
            catch (Exception)
            {
                // ignored
                // load default
                config = new Config();
            }
        }

        public Config GetConfig()
        {
            return config;
        }

        public void Save()
        {
            try
            {
                using (var sw = new StreamWriter(filename))
                {
                    sw.Write(JsonConvert.SerializeObject(config));
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
