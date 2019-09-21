using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;

namespace ImageFramework.Model
{
    public class ImagesModel : INotifyPropertyChanged
    { 
        private struct Dimension
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private Dimension[] dimensions = null;

        public class TextureArray2DInfo
        {
            public TextureArray2D Image { get; set; }
            public string Filename { get; set; }
            public bool IsGrayscale { get; set; }
            public bool IsAlpha { get; set; }
        }

        private class ImageData : IDisposable
        {
            public TextureArray2D Image { get; private set; }
            public int NumMipmaps => Image.NumMipmaps;
            public int NumLayers => Image.NumLayers;
            public bool IsGrayscale { get; }
            public bool HasAlpha { get; }
            public bool IsHdr { get; }
            public string Filename { get; }
            public string FormatName { get; }

            public ImageData(TextureArray2DInfo info)
            {
                Image = info.Image;
                IsGrayscale = info.IsGrayscale;
                HasAlpha = info.IsAlpha;
                IsHdr = true;
                Filename = info.Filename;
                //FormatName = 
                // TODO format name
                throw new NotImplementedException();
            }

            public void GenerateMipmaps(int levels)
            {
                throw new NotImplementedException();
            }

            public void DeleteMipmaps()
            {
                throw new NotImplementedException();
            }


            public void Dispose()
            {
                Image?.Dispose();
            }
        }

        private readonly List<ImageData> images = new List<ImageData>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
