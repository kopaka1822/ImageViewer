using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Image : IDisposable
    {
        // image format
        public ImageFormat Format { get; }
        // format of the source image (can be different form current format)
        public GliFormat OriginalFormat { get; }
        public List<Layer> Layers { get; }
        public string Filename { get; }

        // link to the dll resource id
        public Resource Resource { get; }

        public Image(Resource resource, string filename, int nLayer, int nMipmaps, ImageFormat format, GliFormat originalFormat)
        {
            Resource = resource;
            Filename = filename;
            Format = format;
            OriginalFormat = originalFormat;
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

        public int GetDepth(int mipmap)
        {
            if (Layers.Count > 0 && (uint)mipmap < Layers[0].Mipmaps.Count)
                return Layers[0].Mipmaps[mipmap].Depth;
            return 0;
        }

        public int NumMipmaps => Layers.Count > 0 ? Layers[0].Mipmaps.Count : 0;
        public int NumLayers => Layers.Count;

        public bool Is3D => NumMipmaps > 0 && Layers[0].Mipmaps[0].Depth > 1;

        public void Dispose()
        {
            Resource.Dispose();
        }
    }
}
