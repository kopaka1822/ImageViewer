using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Settings;
using ImageViewer.UtilityEx;
using Microsoft.Win32;

namespace ImageViewer.Commands.Export
{
    class ExportConfigCommand : SimpleCommand
    {
        private readonly ModelsEx models;
        private readonly PathManager path;

        public ExportConfigCommand(ModelsEx models)
        {
            this.models = models;
            this.path = models.ViewerConfigPath;
        }

        public override void Execute()
        {
            if (path.Filename == null) // set proposed filename from equations
                path.InitFromEquations(models);

            // make sure that the directory exists
            path.CreateDirectory();

            var sfd = new SaveFileDialog
            {
                Filter = "ICFG (*.icfg)|*.icfg",
                InitialDirectory = path.Directory,
                FileName = path.Filename
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            var cfg = ViewerConfig.LoadFromModels(models, ViewerConfig.Components.All);

            path.UpdateFromFilename(sfd.FileName);

            cfg.WriteToFile(path.FullPath);
        }
    }
}
