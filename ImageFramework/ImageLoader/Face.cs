using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Face
    {
        public List<Mipmap> Mipmaps { get; }

        public Face(Resource resource, int imageId, int layerId, int nMipmaps)
        {
            Mipmaps = new List<Mipmap>(nMipmaps);
            for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
            {
                Mipmaps.Add(new Mipmap(resource, imageId, layerId, curMipmap));
            }
        }
    }
}
