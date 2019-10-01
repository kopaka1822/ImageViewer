using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Controller;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    public class Models : IDisposable
    {
        public static readonly CultureInfo Culture = new CultureInfo("en-US");
        public ImagesModel Images { get; }

        public IReadOnlyList<FinalImageModel> FinalImages { get; }

        internal ShaderModel Shader { get; }
        internal DxModel DxModel { get; }
        internal TextureCacheModel TexCache { get; }

        private readonly List<FinalImageModel> finalImages = new List<FinalImageModel>();
        private readonly List<FinalImageController> finalImageControllers = new List<FinalImageController>();

        public Models()
        {
            Shader = new ShaderModel();
            DxModel = new DxModel();
            Images = new ImagesModel();
            TexCache = new TextureCacheModel(Images);

            FinalImages = finalImages;
            // add one image equation by default
            AddFinalImage();
        }

        public void AddFinalImage()
        {
            var fi = new FinalImageModel();
            finalImages.Add(fi);
            finalImageControllers.Add(new FinalImageController(this, fi));
        }

        public void Dispose()
        {
            Shader?.Dispose();
            Images?.Dispose();
            TexCache?.Dispose();
        }
    }
}
