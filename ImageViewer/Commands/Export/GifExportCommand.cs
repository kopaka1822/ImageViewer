using System.ComponentModel;
using System.IO;
using ImageFramework.DirectX;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;
using Microsoft.Win32;

namespace ImageViewer.Commands.Export
{
    public class GifExportCommand : Command
    {
        private readonly ModelsEx models;
        private readonly GifExportViewModel viewModel = new GifExportViewModel();
        private readonly PathManager path;

        public GifExportCommand(ModelsEx models)
        {
            this.models = models;
            this.path = models.ExportPath;
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

            path.InitFromEquations(models);

            var sfd = new SaveFileDialog
            {
                Filter = "MPEG-4 (*.mp4)|*.mp4",
                InitialDirectory = path.Directory,
                FileName = path.Directory
            };

            if (sfd.ShowDialog(models.Window.TopmostWindow) != true)
                return;

            path.UpdateFromFilename(sfd.FileName, updateExtension: false);

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
