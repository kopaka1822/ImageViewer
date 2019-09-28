using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Image : IDisposable
    {
        public ImageFormat Format { get; }
        public List<Layer> Layers { get; }
        public string Filename { get; }

        // link to the dll resource id
        public Resource Resource { get; }

        public Image(Resource resource, string filename, int nLayer, int nMipmaps, ImageFormat format)
        {
            Resource = resource;
            Filename = filename;
            Format = format;
            // load relevant information

            Layers = new List<Layer>(nLayer);
            for (var curLayer = 0; curLayer < nLayer; ++curLayer)
            {
                Layers.Add(new Layer(resource, curLayer, nMipmaps));
            }
        }

        public int GetWidth(int mipmap)
        {
            if (Layers.Count > 0 && (uint)mipmap < Layers[0].Mipmaps.Count)
                return Layers[0].Mipmaps[mipmap].Width;
            return 0;
        }

        public int GetHeight(int mipmap)
        {
            if (Layers.Count > 0 && (uint)mipmap < Layers[0].Mipmaps.Count)
                return Layers[0].Mipmaps[mipmap].Height;
            return 0;
        }

        public int NumMipmaps => Layers.Count > 0 ? Layers[0].Mipmaps.Count : 0;
        public int NumLayers => Layers.Count;

        public void Dispose()
        {
            Resource.Dispose();
        }
    }
}
