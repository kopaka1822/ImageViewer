using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Layer
    {
        public List<Mipmap> Mipmaps { get; }

        public Layer(Resource resource, int layerId, int nMipmaps)
        {
            Mipmaps = new List<Mipmap>(nMipmaps);
            for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
            {
                Mipmaps.Add(new Mipmap(resource, layerId, curMipmap));
            }
        }
    }
}
