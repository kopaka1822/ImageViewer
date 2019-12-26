using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands
{
    public class GifExportCommand : Command
    {
        private readonly ModelsEx models;
        private readonly GifExportViewModel viewModel = new GifExportViewModel();

        public GifExportCommand(ModelsEx models)
        {
            this.models = models;
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

        public override bool CanExecute()
        {
            return models.Images.NumImages > 0;
        }

        public override void Execute()
        {
            if (models.NumEnabled != 2)
            {
                models.Window.ShowErrorDialog("Exactly two image equations should be visible for exporting");
                return;
            }

            if (models.Images.ImageType != typeof(TextureArray2D))
            {
                models.Window.ShowErrorDialog("Only 2D textures are supported");
                return;
            }

            if (!FFMpeg.IsAvailable())
            {
                models.Window.ShowErrorDialog("ffmpeg is required for this feature. Please download the ffmpeg binaries and place them in the ImageViewer root directory");
                return;
            }

            var ids = models.GetEnabledPipelines();
            // images valid?
            var img1 = models.Pipelines[ids[0]].Image;
            var img2 = models.Pipelines[ids[1]].Image;
            if (img1 == null) return;
            if (img2 == null) return;

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (models.Display.Multiplier != 1.0f)
            {
                models.Window.ShowInfoDialog("Export will ignore image multiplier");
            }

            // export directory fallback
            var firstImageId = models.Pipelines[ids[0]].Color.FirstImageId;
            var firstImage = models.Images.Images[firstImageId].Filename;

            var sfd = new SaveFileDialog
            {
                Filter = "MPEG-4 (*.mp4)|*.mp4",
                InitialDirectory = ExportCommand.GetExportDirectory(firstImage),
                FileName = System.IO.Path.GetFileNameWithoutExtension(firstImage)
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            ExportCommand.SetExportDirectory(System.IO.Path.GetDirectoryName(sfd.FileName));

            var dia = new GifExportDialog(viewModel);
            if (models.Window.ShowDialog(dia) != true) return;
            
            // get tmp directory
            var tmpDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            System.IO.Directory.CreateDirectory(tmpDir);
            var tmpName = tmpDir + "\\frame";

            // delete old file if it existed (otherwise ffmpeg will hang)
            System.IO.File.Delete(sfd.FileName);

            models.Gif.CreateGif((TextureArray2D)img1, (TextureArray2D)img2, new GifModel.Config
            {
                Filename = sfd.FileName,
                TmpFilename = tmpName,
                FramesPerSecond = viewModel.FramesPerSecond,
                SliderWidth = viewModel.SliderSize,
                NumSeconds = viewModel.TotalSeconds
            });
        }
    }
}
