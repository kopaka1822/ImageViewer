using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Progress;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.Commands.Tools
{
    public class GenerateBlueNoiseCommand : SimpleCommand
    {
        public GenerateBlueNoiseCommand(ModelsEx models) : base(models)
        {

        }

        public override async void Execute()
        {
            var lm = new LayerMipmapCount(Math.Max(models.Images.NumLayers, 1), Math.Max(models.Images.NumMipmaps, 1));
            var size = models.Images.Size;
            var is3D = models.Images.Is3D;
            if (size == Size3.Zero) // no image loaded
            {
                lm = new LayerMipmapCount(1, 1);
                size = GenerateWhiteNoiseCommand.ShowNewImageDialogue(models);
                is3D = size.Z > 1;
            }
            if (size == Size3.Zero)
                return; // user canceled

            // generate blue noise texture in c++ DLL
            using (var img = await LoadNoiseAsync(size, lm))
            {
                ITexture tex;
                if (is3D)
                    tex = new Texture3D(img);
                else
                    tex = new TextureArray2D(img);

                // add texture
                models.Images.AddImage(tex, false, img.Filename, img.OriginalFormat, img.Filename);
            }
        }

        private Task<DllImageData> LoadNoiseAsync(Size3 size, LayerMipmapCount lm)
        {
            var task = Task.Run(() => IO.LoadBlueNoise(size, lm));

            var cts = new CancellationTokenSource();
            models.Progress.AddTask(task, cts, true);
            return task;
        }
    }
}
