using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands.Export
{
    public class ExportCommand : Command
    {
        private readonly ModelsEx models;
        private static string s_exportDirectory = null;
        private string exportExtension = null;
        private GliFormat? exportFormat = null;

        public ExportCommand(ModelsEx models)
        {
            this.models = models;
            this.models.PropertyChanged += ModelsOnPropertyChanged;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        public static string GetExportDirectory(string fallbackFilename)
        {
            // set or keep export directory
            if (s_exportDirectory == null)
            {
                s_exportDirectory = System.IO.Path.GetDirectoryName(fallbackFilename);
            }

            return s_exportDirectory;
        }

        public static void SetExportDirectory(string directory)
        {
            if (directory != null)
                s_exportDirectory = directory;
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
            return models.Images.NumImages > 0 && models.NumEnabled > 0;
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
            var tex = models.Pipelines[id].Image;
            if (tex == null) return; // not yet computed?

            float multiplier = 1.0f;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (models.Display.Multiplier != 1.0f)
            {
                if (models.Window.ShowYesNoDialog(
                    $"Color multiplier is currently set to {models.Display.MultiplierString}. Do you want to include the multiplier in the export?",
                    "Keep Color Multiplier?"))
                {
                    multiplier = models.Display.Multiplier;
                }
            }

            // set proposed filename
            var firstImageId = models.Pipelines[id].Color.FirstImageId;
            var firstImage = models.Images.Images[firstImageId].Filename;
            var proposedFilename = System.IO.Path.GetFileNameWithoutExtension(firstImage);
            
            // set or keep export directory
            var exportDirectory = GetExportDirectory(firstImage);

            if (exportExtension == null)
            {
                exportExtension = System.IO.Path.GetExtension(firstImage);
                if (exportExtension != null && exportExtension.StartsWith("."))
                    exportExtension = exportExtension.Substring(1);
            }

            if (exportFormat == null)
            {
                exportFormat = models.Images.Images[firstImageId].OriginalFormat;
            }

            // open save file dialog
            Debug.Assert(exportDirectory != null);
            Debug.Assert(proposedFilename != null);

            var sfd = new SaveFileDialog
            {
                Filter = GetFilter(exportExtension, tex.Is3D),
                InitialDirectory = exportDirectory,
                FileName = proposedFilename
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            exportExtension = System.IO.Path.GetExtension(sfd.FileName).Substring(1);
            var exportFilename = System.IO.Path.GetFileNameWithoutExtension(sfd.FileName);
            exportDirectory = System.IO.Path.GetDirectoryName(sfd.FileName);
            SetExportDirectory(exportDirectory);

            
            var viewModel = new ExportViewModel(models, exportExtension, exportFormat.Value, sfd.FileName, tex.Is3D, models.Statistics[id].Stats);
            var dia = new ExportDialog(viewModel);

            if (models.Window.ShowDialog(dia) != true) return;

            var desc = new ExportDescription(tex, exportDirectory + "/" + exportFilename, exportExtension)
            {
                Multiplier = multiplier,
                Mipmap = models.ExportConfig.Mipmap,
                Layer = models.ExportConfig.Layer,
                UseCropping = models.ExportConfig.UseCropping,
                CropStart = models.ExportConfig.CropStart,
                CropEnd = models.ExportConfig.CropEnd,
                Overlay = models.Overlay.Overlay
            };
            desc.TrySetFormat(viewModel.SelectedFormatValue);
            exportFormat = desc.FileFormat;

            models.Export.ExportAsync(desc);

            // export additional zoom boxes?
            if (viewModel.HasZoomBox && viewModel.ExportZoomBox)
            {
                for (int i = 0; i < models.ZoomBox.Boxes.Count; ++i)
                {
                    var box = models.ZoomBox.Boxes[i];
                    var zdesc = new ExportDescription(tex, $"{exportDirectory}/{exportFilename}_zoom{i}",
                        exportExtension)
                    {
                        Multiplier = multiplier,
                        Mipmap = models.ExportConfig.Mipmap,
                        Layer = models.ExportConfig.Layer,
                        UseCropping = true,
                        CropStart = new Float3(box.Start, 0.0f),
                        CropEnd = new Float3(box.End, 1.0f),
                        Overlay = viewModel.ZoomBorders ? models.Overlay.Overlay : null,
                        Scale = viewModel.ZoomBoxScale
                    };
                    zdesc.TrySetFormat(viewModel.SelectedFormatValue);

                    await models.Progress.WaitForTaskAsync();
                    models.Export.ExportAsync(zdesc);
                }
            }
            
        }

        private static Dictionary<string, string> filter = new Dictionary<string, string>
        {
            {"png", "PNG (*.png)|*.png" },
            {"bmp", "BMP (*.bmp)|*.bmp" },
            {"jpg", "JPEG (*.jpg)|*.jpg" },
            {"hdr", "HDR (*.hdr)|*.hdr" },
            {"pfm", "Portable float map (*.pfm)|*.pfm" },
            {"ktx", "Khronos Texture (*.ktx)|*.ktx" },
            {"dds", "DirectDraw Surface (*.dds)|*.dds" },
        };

        private static bool Is3DFilter(string key)
        {
            return key == "dds" || key == "ktx";
        }

        private static string GetFilter(string preferred, bool is3D)
        {
            string pref = null;
            if(!is3D || Is3DFilter(preferred))
                filter.TryGetValue(preferred, out pref);

            var res = "";
            if (pref != null)
                res += pref + "|";

            foreach (var f in filter)
            {
                if (f.Value != pref && (!is3D || Is3DFilter(f.Key)))
                    res += f.Value + "|";
            }

            return res.TrimEnd('|');
        }
    }
}
