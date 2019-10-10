using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands
{
    public class ExportCommand : Command
    {
        private readonly ModelsEx models;
        private string exportDirectory = null;
        private string exportExtension = null;
        private GliFormat? exportFormat = null;

        public ExportCommand(ModelsEx models)
        {
            this.models = models;
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
            return models.Images.NumImages > 0 && models.NumEnabled > 0;
        }

        public override void Execute()
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
            if (exportDirectory == null)
            {
                exportDirectory = System.IO.Path.GetDirectoryName(firstImage);
            }

            if (exportExtension == null)
            {
                exportExtension = System.IO.Path.GetExtension(firstImage);
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
                Filter = GetFilter(exportExtension),
                InitialDirectory = exportDirectory,
                FileName = proposedFilename
            };

            if (sfd.ShowDialog() != true)
                return;

            exportExtension = System.IO.Path.GetExtension(sfd.FileName);
            exportDirectory = System.IO.Path.GetDirectoryName(sfd.FileName);

            var viewModel = new ExportViewModel(models, exportExtension, exportFormat.Value, sfd.FileName);
            var dia = new ExportDialog(viewModel);

            if (models.Window.ShowDialog(dia) != true) return;

            var desc = new ExportDescription(sfd.FileName, exportExtension, models.Export);
            desc.TrySetFormat(viewModel.SelectedFormatValue);

            try
            {
                models.Export.Export(tex, desc);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e.Message, "during export");
            }

        }

        private static Dictionary<string, string> filter = new Dictionary<string, string>
        {
            {".png", "PNG (*.png)|*.png" },
            {".bmp", "BMP (*.bmp)|*.bmp" },
            {".jpg", "JPEG (*.jpg)|*.jpg" },
            {".hdr", "HDR (*.hdr)|*.hdr" },
            {".pfm", "Portable float map (*.pfm)|*.pfm" },
            {".dds", "Khronos Texture (*.ktx)|*.ktx" },
            {".ktx", "DirectDraw Surface (*.dds)|*.dds" },
        };

        private static string GetFilter(string preferred)
        {
            filter.TryGetValue(preferred, out var pref);

            var res = "";
            if (pref != null)
                res += pref + "|";

            foreach (var f in filter)
            {
                if (f.Value != pref)
                    res += f.Value + "|";
            }

            return res.TrimEnd('|');
        }
    }
}
