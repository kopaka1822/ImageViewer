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
    public class GenerateBlueNoiseCommand : Command
    {
        public GenerateBlueNoiseCommand(ModelsEx models) : base(models)
        {
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

        public override async void Execute()
        {
            // generate blue noise texture in c++ DLL
            using (var img = await LoadNoiseAsync())
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

        private Task<DllImageData> LoadNoiseAsync()
        {
            var task = Task.Run(() => IO.LoadBlueNoise(models.Images.Size,
                new LayerMipmapCount(models.Images.NumLayers, models.Images.NumMipmaps)));

            var cts = new CancellationTokenSource();
            models.Progress.AddTask(task, cts, true);
            return task;
        }
    }
}
