using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Controller;
using ImageViewer.Models;

namespace ImageViewer.Commands
{
    public class OpenCommand : SimpleCommand
    {
        private readonly ModelsEx models;
        private readonly ImportDialogController import;

        public OpenCommand(ModelsEx models)
        {
            this.models = models;
            this.import = new ImportDialogController(models);
        }

        public override async void Execute()
        {
            var files = import.ShowImportImageDialog();
            if (files == null) return;

            // nothing imported yet => show in same window
            if (models.Images.NumImages == 0)
            {
                foreach (var file in files)
                {
                    await import.ImportImageAsync(file);
                }
                return;
            }

            // already images imported => show in new window
            var args = "";
            foreach (var file in files)
            {
                args += $"\"{file}\" ";
            }

            var info = new System.Diagnostics.ProcessStartInfo(models.Window.AssemblyPath, args);
            System.Diagnostics.Process.Start(info);
        }
    }
}
