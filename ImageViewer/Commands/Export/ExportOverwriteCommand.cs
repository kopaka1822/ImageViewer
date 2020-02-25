using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Export
{
    class ExportOverwriteCommand : Command, INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public ExportOverwriteCommand(ModelsEx models)
        {
            this.models = models;
            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += PipeOnPropertyChanged;
            }
        }

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.IsEnabled):
                case nameof(ImagePipeline.Image):
                    OnCanExecuteChanged();
                    OnPropertyChanged(nameof(Text));
                    break;
            }
        }

        public override bool CanExecute()
        {
            if (models.NumEnabled != 1) return false;
            var pipe = models.Pipelines[models.GetFirstEnabledPipeline()];
            // is computed?
            if (pipe.Image == null) return false;
            // is a file?
            var imgId = GetFirstImageId(pipe);
            if (!models.Images.Images[imgId].IsFile) return false;

            return true;
        }

        public string Text
        {
            get
            {
                if (!CanExecute()) return "Overwrite ...";

                var imgId = GetFirstImageId(models.Pipelines[models.GetFirstEnabledPipeline()]);
                var filename = Path.GetFileName(models.Images.Images[imgId].Filename);
                return $"Overwrite {filename}";
            }
        }

        public override void Execute()
        {
            var pipe = models.Pipelines[models.GetFirstEnabledPipeline()];
            // not yet computed
            var tex = pipe.Image;
            if (tex == null) return;

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
            var firstImageId = GetFirstImageId(pipe);
            var filename = models.Images.Images[firstImageId].Filename;
            var format = models.Images.Images[firstImageId].OriginalFormat;
            var ext = Path.GetExtension(filename);
            if (ext == null) return;
            // remove . from extension
            if (ext.StartsWith(".")) ext = ext.Substring(1);

            var path = Path.ChangeExtension(filename, null);

            var desc = new ExportDescription(tex, path, ext)
            {
                Multiplier = multiplier,
            };
            desc.TrySetFormat(format);

            models.Export.ExportAsync(desc);
        }

        private int GetFirstImageId(ImagePipeline pipe)
        {
            if(pipe.Color.HasImages)
                return pipe.Color.FirstImageId;
            return pipe.Alpha.FirstImageId;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
