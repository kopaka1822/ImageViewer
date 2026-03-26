using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ImageViewer.Commands.Export
{
    public class ExportFramesCommand : Command
    {
        private readonly PathManager path;

        public ExportFramesCommand(ModelsEx models) : base(models)
        {
            path = models.ExportConfig.Path;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumLayers):
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute()
        {
            return models.Images.NumLayers > 1;
        }

        public override async Task ExecuteAsync()
        {
            // Get active final image
            var pipeId = models.GetExportPipelineId();
            var pipe = models.Pipelines[pipeId];
            var tex = pipe.Image;
            if (tex == null || !pipe.IsValid) return;

            float multiplier = ExportCommand.GetImageMultiplier(models);

            path.InitFromEquations(models);

            if (models.ExportConfig.Format == null)
            {
                var firstImageId = pipe.Color.FirstImageId;
                models.ExportConfig.Format = models.Images.Images[firstImageId].OriginalFormat;
            }

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

            // EXTENSION SELECT
            ExportFramesViewModel framesViewModel;
            try
            {
                framesViewModel = new ExportFramesViewModel(models, tex.Is3D, path, tex.NumLayers);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }

            var framesDia = new ExportFramesDialog(framesViewModel);

            if (models.Window.ShowDialog(framesDia) != true) return;

            path.Extension = framesViewModel.ExportExtension;

            // OUTPUT FORMAT SELECT (SECOND DIALOG)
            ExportViewModel viewModel;
            try
            {
                viewModel = new ExportViewModel(models, path.Extension, models.ExportConfig.Format.Value, 
                    $"{tex.NumLayers} frames", tex.Is3D, models.Statistics[pipeId].Stats);
                // Always select "All Layers" for frames export
                viewModel.AvailableLayers.Clear(); 
                viewModel.AvailableLayers.Add(new ViewModels.ListItemViewModel<int>{
                    Cargo = 0,
                    Name = "All Layers"
                });
                viewModel.SelectedLayer = viewModel.AvailableLayers.FirstOrDefault();
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
                return;
            }

            var dia = new ExportDialog(viewModel);

            if (models.Window.ShowDialog(dia) != true) return;

            models.ExportConfig.Format = viewModel.SelectedFormatValue;

            // Export each layer as a separate frame
            await ExportFramesAsync(tex, path, multiplier, models, viewModel);
        }

        private async Task ExportFramesAsync(ITexture tex, PathManager path, float multiplier, ModelsEx models, ExportViewModel viewModel)
        {
            var numLayers = tex.NumLayers;
            var baseFilename = path.Filename ?? "frame";
            
            // Remove extension if present
            if (baseFilename.EndsWith(path.Extension))
                baseFilename = baseFilename.Substring(0, baseFilename.Length - path.Extension.Length);

            int padding = 4; // hardcode to 4 because max layers is 2048

            for (int layer = 0; layer < numLayers; layer++)
            {
                // Generate filename: frame0000, frame0001, etc.
                var frameFilename = $"{baseFilename}{layer.ToString().PadLeft(padding, '0')}";
                
                var desc = new ExportDescription(tex, path.Directory + "/" + frameFilename, path.Extension)
                {
                    Multiplier = multiplier,
                    Mipmap = models.ExportConfig.Mipmap,
                    Layer = layer,
                    UseCropping = models.ExportConfig.UseCropping,
                    CropStart = models.ExportConfig.CropStart,
                    CropEnd = models.ExportConfig.CropEnd,
                    Overlay = models.Overlay.Overlay,
                    Quality = models.Settings.LastQuality,
                    Fps = models.Settings.MovieFps,
                };
                desc.TrySetFormat(viewModel.SelectedFormatValue);

                models.Export.ExportAsync(desc);
                
                // Wait for the export to complete before starting the next one
                await models.Progress.WaitForTaskAsync();
                
                // Check if there was an error
                if (!string.IsNullOrEmpty(models.Progress.LastError))
                {
                    models.Window.ShowErrorDialog($"Error exporting frame {layer}: {models.Progress.LastError}");
                    return;
                }
            }

            // Export zoom boxes if enabled
            if (viewModel.HasZoomBox && viewModel.ExportZoomBox)
            {
                for (int layer = 0; layer < numLayers; layer++)
                {
                    var frameFilename = $"{baseFilename}{layer.ToString().PadLeft(padding, '0')}";
                    
                    for (int i = 0; i < models.ZoomBox.Boxes.Count; ++i)
                    {
                        var box = models.ZoomBox.Boxes[i];
                        var zdesc = new ExportDescription(tex, $"{path.Directory}/{frameFilename}_zoom{i}", path.Extension)
                        {
                            Multiplier = multiplier,
                            Mipmap = models.ExportConfig.Mipmap,
                            Layer = layer,
                            UseCropping = true,
                            CropStart = new Float3(box.Start, 0.0f),
                            CropEnd = new Float3(box.End, 1.0f),
                            Overlay = viewModel.ZoomBorders ? models.Overlay.Overlay : null,
                            Scale = viewModel.ZoomBoxScale,
                            Quality = models.Settings.LastQuality,
                            Fps = models.Settings.MovieFps,
                        };
                        zdesc.TrySetFormat(viewModel.SelectedFormatValue);

                        await models.Progress.WaitForTaskAsync();
                        models.Export.ExportAsync(zdesc);
                    }
                }
                
                await models.Progress.WaitForTaskAsync();
            }
        }
    }
}