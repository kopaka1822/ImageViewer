using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels;
using Microsoft.Win32;

namespace ImageViewer.Commands
{
    public class AddFilterCommand : SimpleCommand
    {
        private readonly ModelsEx models;
        private readonly FiltersViewModel viewModel;
        private readonly PathManager path = new PathManager();

        public AddFilterCommand(ModelsEx models, FiltersViewModel viewModel)
        {
            this.models = models;
            this.viewModel = viewModel;

        }

        public override void Execute()
        {
            path.UpdateDirectory(models.Settings.FilterPath, models.Window.ExecutionPath + "\\Filter");

            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = path.Directory
            };

            if (ofd.ShowDialog(models.Window.TopmostWindow) != true) return;
            models.Settings.FilterPath = System.IO.Path.GetDirectoryName(ofd.FileName);

            // create model
            try
            {
                var model = models.CreateFilter(ofd.FileName);

                // add to filter list
                viewModel.AddFilter(model);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
        }
    }
}
