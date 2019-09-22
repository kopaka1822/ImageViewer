using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Image
    {
        public ImageFormat Format { get; }
        public List<Layer> Layers { get; }
        public string Filename { get; }

        public Image(Resource resource, string filename, int nLayer, int nMipmaps, ImageFormat format)
        {
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

        /// <summary>
        /// checks if the image is grayscale
        /// </summary>
        /// <returns>true if only one component is used</returns>
        public bool IsGrayscale()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// checks for availability of the alpha components
        /// </summary>
        /// <returns>true if image has an alpha component</returns>
        public bool HasAlpha()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// checks for high dimension range
        /// </summary>
        /// <returns>true if the image type is bigger than byte (range > [0-255])</returns>
        public bool IsHdr()
        {
           throw new NotImplementedException();
        }
    }
}
