using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Commands.Tools
{
    

    public class GenerateWhiteNoiseCommand : SimpleCommand
    {
        public GenerateWhiteNoiseCommand(ModelsEx models) : base(models)
        {
            
        }

        public override void Execute()
        {
            var lm = new LayerMipmapCount(Math.Max(models.Images.NumLayers, 1), Math.Max(models.Images.NumMipmaps, 1));
            var size = models.Images.Size;
            var is3D = models.Images.Is3D;
            if (size == Size3.Zero) // no image loaded
            {
                lm = new LayerMipmapCount(1, 1);
                size = ShowNewImageDialogue(models);
                is3D = size.Z > 1;
            }
            if(size == Size3.Zero)
                return; // user canceled

            // generate white noise texture in c++ DLL
            using (var img = IO.LoadWhiteNoise(size, lm, 0))
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

        public static Size3 ShowNewImageDialogue(ModelsEx models)
        {
            var vm = new SimpleImageViewModel();
            var dialog = new SimpleImageDialog(vm);
            if (models.Window.ShowDialog(dialog) == true)
            {
                return new Size3(vm.ImageWidth, vm.ImageHeight, vm.ImageDepth);
            }

            return new Size3(0);
        }
    }
}
