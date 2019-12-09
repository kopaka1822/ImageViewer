using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageViewer.Models;

namespace ImageViewer.Controller
{
    /// <summary>
    /// handles drop events in the client area
    /// </summary>
    public class ClientDropController
    {
        private readonly ModelsEx models;
        private readonly ImportDialogController import;

        public ClientDropController(ModelsEx models)
        {
            this.models = models;
            var dxHost = models.Window.Window.BorderHost;
            dxHost.DragOver += (o, args) => args.Effects = DragDropEffects.Copy;
            dxHost.AllowDrop = true;
            dxHost.Drop += DxHostOnDrop;

            import = new ImportDialogController(models);
        }

        private async void DxHostOnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null)
                    foreach (var file in files)
                        await import.ImportImageAsync(file);
            }
        }
    }
}
