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

namespace ImageViewer.Commands.Tools
{
    

    public class GenerateWhiteNoiseCommand : Command
    {
        public GenerateWhiteNoiseCommand(ModelsEx models) : base(models)
        {
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
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
            // generate white noise texture in c++ DLL
            using (var img = IO.LoadWhiteNoise(models.Images.Size,
                       new LayerMipmapCount(models.Images.NumLayers, models.Images.NumMipmaps), 0))
            {
                ITexture tex;
                if (models.Images.Is3D)
                    tex = new Texture3D(img);
                else
                    tex = new TextureArray2D(img);

                // add texture
                models.Images.AddImage(tex, false, img.Filename, img.OriginalFormat, img.Filename);
            }
        }
    }
}
