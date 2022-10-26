using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageViewer.Models;
using ImageViewer.Models.Settings;

namespace ImageViewer.Controller
{
    /// <summary>
    /// handles drop events in the client area
    /// </summary>
    public class ClientDropController
    {
        private readonly ModelsEx models;

        public ClientDropController(ModelsEx models)
        {
            this.models = models;
            var dxHost = models.Window.Window.BorderHost;
            dxHost.DragOver += (o, args) => args.Effects = DragDropEffects.Copy;
            dxHost.AllowDrop = true;
            dxHost.Drop += DxHostOnDrop;
        }

        private async void DxHostOnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files == null) return;
            
            foreach (var file in files)
            {
                if (file.EndsWith(".icfg"))
                {
                    var cfg = ViewerConfig.LoadFromFile(file);
                    await cfg.ApplyToModels(models);
                }
                else await models.Import.ImportImageAsync(file);
            }
        }
    }
}
