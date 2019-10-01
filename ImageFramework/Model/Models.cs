using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    public class Models : IDisposable
    {
        public static readonly CultureInfo Culture = new CultureInfo("en-US");
        public ImagesModel Images { get; }
        internal ShaderModel Shader { get; }
        internal DxModel DxModel { get; }
        internal TextureCacheModel TexCache { get; }

        public Models()
        {
            Shader = new ShaderModel();
            DxModel = new DxModel();
            Images = new ImagesModel();
            TexCache = new TextureCacheModel(Images);
        }

        public void Dispose()
        {
            Shader?.Dispose();
            Images?.Dispose();
            TexCache?.Dispose();
        }
    }
}
