using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model;
using ImageFramework.Model.Equation;
using ImageFramework.Utility;

namespace ImageFramework.Controller
{
    /// <summary>
    /// controller that assures that the pipeline images are up to date
    /// </summary>
    internal class PipelineController : IDisposable
    {
        private readonly Models models;
        private readonly TextureCache textureCache;
        private readonly UploadBuffer<LayerLevelData> layerLevelBuffer;

        public PipelineController(Models models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;

            foreach (var pipe in models.Pipelines)
            {
                pipe.PropertyChanged += (sender, e) => PipelineOnPropertyChanged(pipe, e);
                pipe.Color.PropertyChanged += (sender, e) => PipelineFormulaOnPropertyChanged(pipe, pipe.Color, e);
                pipe.Alpha.PropertyChanged += (sender, e) => PipelineFormulaOnPropertyChanged(pipe, pipe.Alpha, e);
            }

            textureCache = new TextureCache(models.Images);
            layerLevelBuffer = new UploadBuffer<LayerLevelData>(1);
        }

        private void PipelineFormulaOnPropertyChanged(ImagePipeline pipe, FormulaModel formula, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FormulaModel.Converted):
                    // verify if the new formula is still valid
                    UpdateFormulaValidity(pipe, models.Images.NumImages);
                    pipe.HasChanges = true;
                    break;
            }
        }

        public async Task UpdateImagesAsync(CancellationToken ct)
        {
            var args = new ImagePipeline.UpdateImageArgs
            {
                Images = models.Images,
                LayerLevelBuffer = layerLevelBuffer,
                Progress = models.Progress,
                TextureCache = textureCache
            };

            foreach (var pipe in models.Pipelines)
            {
                if (pipe.HasChanges && pipe.IsValid)
                {
                    await pipe.UpdateImageAsync(args, ct);
                }
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    // update valid status
                    foreach (var pipe in models.Pipelines)
                    {
                        UpdateFormulaValidity(pipe, models.Images.NumImages);
                    }
                    break;
                case nameof(ImagesModel.ImageOrder):
                case nameof(ImagesModel.NumMipmaps):
                    // all images must be recomputed
                    foreach (var pipe in models.Pipelines)
                    {
                        pipe.HasChanges = true;
                    }
                    break;
            }
        }

        private void PipelineOnPropertyChanged(ImagePipeline sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.HasChanges):
                    if (sender.HasChanges)
                    {
                        sender.ResetImage(textureCache);
                    }
                    break;
                case nameof(ImagePipeline.IsValid):
                    if (sender.IsValid && sender.HasChanges)
                    {
                        // TODO shedule work?
                    }
                    break;
            }
        }

        private void UpdateFormulaValidity(ImagePipeline pipe, int numImages)
        {
            pipe.IsValid = pipe.Color.MaxImageId < numImages && pipe.Alpha.MaxImageId < numImages;
        }

        public void Dispose()
        {
            textureCache?.Dispose();
            layerLevelBuffer?.Dispose();
        }
    }
}
