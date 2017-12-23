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
            public string ImagePath = "";
            public string TonemapperPath = "";
            public string ExportPath = "";
            public int WindowSizeX = 800;
            public int WindowSizeY = 600;
            public bool IsMaximized = false;
            // relative coordinates to the main window
            public int LastImageDialogX = 0;
            public int LastImageDialogY = 0;
            public int LastTonemapDialogX = 0;
            public int LastTonemapDialogY = 0;
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
