using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Settings;
using Microsoft.Win32;

namespace ImageViewer.Commands.Export
{
    class ExportConfigCommand : SimpleCommand
    {
        private readonly ModelsEx models;

        public ExportConfigCommand(ModelsEx models)
        {
            this.models = models;
        }

        public override void Execute()
        {
            var sfd = new SaveFileDialog
            {
                Filter = "ICFG (*.icfg)|(*icfg)",
                //InitialDirectory = ""
                //FileName = ""
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            var cfg = ViewerConfig.LoadFromModels(models, ViewerConfig.Components.All);
            cfg.WriteToFile(sfd.FileName);
        }
    }
}
