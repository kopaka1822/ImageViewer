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
        public ShaderModel Shader { get; }

        public Models()
        {
            Shader = new ShaderModel();
            Images = new ImagesModel();
        }

        public void Dispose()
        {
            Shader?.Dispose();
        }
    }
}
