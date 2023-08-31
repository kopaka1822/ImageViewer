using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            var pipeId = models.GetFirstEnabledPipeline();
            var pipe = models.Pipelines[pipeId];
            if(pipe.Image == null || !pipe.IsValid) return; // not yet computed?

            if (exportFormat == null)
            {
                var firstImageId = pipe.Color.FirstImageId;
                exportFormat = models.Images.Images[firstImageId].OriginalFormat;
            }

            // check if pipeline is compatible with batch export
            if (!pipe.Color.HasImages)
            {
                models.Window.ShowErrorDialog("Batch export is only supported if images are used in the color equation");
                return;
            }

            float multiplier = ExportCommand.GetImageMultiplier(models);
            path.InitFromEquations(models);

            // DIRECTORY SELECT

            var fd = new CommonOpenFileDialog
            {
                InitialDirectory = path.Directory,
                IsFolderPicker = true,
                Title = "Select export directory",
                EnsurePathExists = true,
                Multiselect = false,
            };

            if (fd.ShowDialog(models.Window.TopmostWindow) != CommonFileDialogResult.Ok)
                return;

            path.Directory = fd.FileName;

            // BATCH FILE SELECT

            ExportBatchViewModel batchViewModel;
            try
            {
                batchViewModel = new ExportBatchViewModel(models, models.Images.Is3D, path);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }

            var batchDia = new ExportBatchDialog(batchViewModel);

            if (models.Window.ShowDialog(batchDia) != true) return;

            var batchIDs = batchViewModel.GetSelectedImages();
            Debug.Assert(batchIDs.Count > 0);
            path.InitFromFilename(models.Images.Images[batchIDs.First()].Filename); // TODO workaround if file has empty filename (use alias?/always use alias?)
            path.Extension = batchViewModel.ExportExtension;

            // OUTPUT FORMAT SELECT

            ExportViewModel viewModel;
            try
            {
                viewModel = new ExportViewModel(models, path.Extension, exportFormat.Value, path.Filename, models.Images.Is3D, models.Statistics[pipeId].Stats);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }

            var dia = new ExportDialog(viewModel);

            if (models.Window.ShowDialog(dia) != true) return;
            exportFormat = viewModel.SelectedFormatValue;

            foreach (var batchId in batchIDs)
            {
                using (var newPipe = pipe.Clone())
                {
                    // TODO replace image id with batch id in equations
                    newPipe.Color.Formula = $"I{batchId}";
                    newPipe.Alpha.Formula = $"I{batchId}";
                    newPipe.SetValid();
                    // TODO make sure we have a valid (and potentially unique) filename
                    path.InitFromFilename(models.Images.Images[batchId].Filename);

                    // calculate new image
                    await newPipe.UpdateImagePipelineAsync(models, pipeId);
                    Debug.Assert(newPipe.Image != null);
                    await ExportCommand.ExportTexAsync(newPipe.Image, path, multiplier, models, viewModel);
                }
            }
        }
    }
}
