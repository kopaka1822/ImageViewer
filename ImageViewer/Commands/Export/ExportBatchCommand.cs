using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SharpDX.D3DCompiler;
using static System.Net.Mime.MediaTypeNames;

namespace ImageViewer.Commands.Export
{
    public class ExportBatchCommand : Command
    {
        private readonly ModelsEx models;
        private GliFormat? exportFormat = null;
        private readonly PathManager path;
        public ExportBatchCommand(ModelsEx models)
        {
            this.models = models;
            path = models.ExportPath;
            this.models.PropertyChanged += ModelsOnPropertyChanged;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    OnCanExecuteChanged();
                    break;
            }
        }

        private void ModelsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImageFramework.Model.Models.NumEnabled):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumImages > 1 && models.NumEnabled > 0;
        }

        public override async void Execute()
        {
            // make sure only one image is visible
            if (models.NumEnabled != 1)
            {
                models.Window.ShowErrorDialog("Exactly one image equation should be visible when exporting.");
                return;
            }

            // get active final image
            var id = models.GetFirstEnabledPipeline();
            var pipe = models.Pipelines[id];
            if(pipe.Image == null) return; // not yet computed?
            
            // check if pipeline is compatible with batch export
            if (!pipe.Color.HasImages)
            {
                models.Window.ShowErrorDialog("Batch export is only supported if images are used in the color equation");
                return;
            }

            float multiplier = ExportCommand.GetImageMultiplier(models);
            path.InitFromEquations(models);



            var fd = new CommonOpenFileDialog
            {
                InitialDirectory = path.Directory,
                IsFolderPicker = true
            };

            if (fd.ShowDialog(models.Window.TopmostWindow) != CommonFileDialogResult.Ok)
                return;

            path.Directory = fd.FileName;

            // TODO select extension and which files to export

            // default file export dialog
            // TODO update path.Filename to the first export image
            // TODO set export format

            ExportViewModel viewModel;
            try
            {
                viewModel = new ExportViewModel(models, path.Extension, exportFormat.Value, path.Filename, models.Images.Is3D, models.Statistics[id].Stats);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }

            var dia = new ExportDialog(viewModel);

            if (models.Window.ShowDialog(dia) != true) return;
            exportFormat = viewModel.SelectedFormatValue;

            // TODO export all images

            // FOR ...
            await ExportCommand.ExportTexAsync(null, path, multiplier, models, viewModel);
        }
    }
}
