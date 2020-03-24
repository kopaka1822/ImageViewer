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

namespace ImageViewer.Commands.Import
{
    public class ReplaceEquationImageCommand : Command<int>
    {
        private readonly ModelsEx models;

        public ReplaceEquationImageCommand(ModelsEx models)
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
                case nameof(ImagePipeline.IsValid):
                case nameof(ImagePipeline.Image): // this catches all of the formula cases
                    OnCanExecuteChanged();
                    break;
            }
        }

        public override bool CanExecute(int parameter)
        {
            var pipe = models.Pipelines[parameter];
            if (!pipe.IsValid || !pipe.IsEnabled || pipe.Image == null) return false;

            // color and alpha may only use one image in their formulas
            if (pipe.Color.MinImageId != pipe.Color.MaxImageId) return false;
            if (pipe.Alpha.MinImageId != pipe.Alpha.MaxImageId) return false;
            if (pipe.Color.MinImageId != pipe.Alpha.MinImageId) return false;

            return true;
        }

        public override void Execute(int parameter)
        {
            var pipe = models.Pipelines[parameter];

            var img = pipe.EjectImage();
            var imgIdx = pipe.Color.MinImageId;
            if (!pipe.Color.HasImages)
                imgIdx = pipe.Alpha.MinImageId;
            
            pipe.Color.Formula = "I" + imgIdx;
            pipe.Alpha.Formula = "I" + imgIdx;

            models.Images.ReplaceImage(imgIdx, img, GliFormat.RGBA32_SFLOAT);

            models.ApplyAsync();
        }
    }
}
