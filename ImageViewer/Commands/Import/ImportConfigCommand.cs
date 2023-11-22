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

namespace ImageViewer.Commands.Import
{
    public class ImportConfigCommand : SimpleCommand
    {
        private readonly PathManager path;

        public ImportConfigCommand(ModelsEx models) : base(models)
        {
            this.path = models.ViewerConfigPath;
        }

        public override async void Execute()
        {
            if (path.Filename == null) // set proposed filename from equations
                path.InitFromEquations(models);
            
            var ofd = new OpenFileDialog
            {
                Filter = "ICFG (*.icfg)|*.icfg",
                InitialDirectory = path.Directory,
                FileName = path.Filename
            };

            if (ofd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            path.UpdateFromFilename(ofd.FileName);

            try
            {
                var cfg = ViewerConfig.LoadFromFile(ofd.FileName);
                await cfg.ApplyToModels(models);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e, "Could not load config");
            }
        }
    }
}
