using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.Models.Settings;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands.Export
{
    class ExportConfigCommand : SimpleCommand
    {
        private readonly ModelsEx models;
        private readonly PathManager path;
        private readonly ExportConfigViewModel viewModel = new ExportConfigViewModel();

        public ExportConfigCommand(ModelsEx models)
        {
            this.models = models;
            this.path = models.ViewerConfigPath;
        }

        public override void Execute()
        {
            // show export dialog
            var dia = new ExportConfigDialog(viewModel);
            if (models.Window.ShowDialog(dia) != true)
                return;

            Debug.Assert(viewModel.IsValid);

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

            try
            {
                var cfg = ViewerConfig.LoadFromModels(models, viewModel.UsedComponents);

                path.UpdateFromFilename(sfd.FileName);

                cfg.WriteToFile(path.FullPath);
            }
            catch(Exception e)
            {
                models.Window.ShowErrorDialog(e, "Could not save config");
            }
        }
    }
}
