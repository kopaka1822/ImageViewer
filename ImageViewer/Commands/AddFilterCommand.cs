using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Filter;
using ImageViewer.Models;
using ImageViewer.ViewModels;
using Microsoft.Win32;

namespace ImageViewer.Commands
{
    public class AddFilterCommand : SimpleCommand
    {
        private readonly ModelsEx models;
        private readonly FiltersViewModel viewModel;

        public AddFilterCommand(ModelsEx models, FiltersViewModel viewModel)
        {
            this.models = models;
            this.viewModel = viewModel;
        }

        public override void Execute()
        {
            var ofd = new OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = models.Settings.FilterPath
            };

            if (ofd.ShowDialog(models.Window.TopmostWindow) != true) return;

            // create model
            var model = models.CreateFilter(ofd.FileName);

            // add to filter list
            viewModel.AddFilter(model);
        }
    }
}
