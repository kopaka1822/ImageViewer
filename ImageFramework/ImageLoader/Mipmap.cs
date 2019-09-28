using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class Mipmap
    {
        public int Width { get; }
        public int Height { get; }

        // this pointer is managed by the resource of the Image class
        public IntPtr Bytes { get; }
        public uint Size { get; }

        public Mipmap(Resource resource, int layerId, int mipmapId)
        {
            Dll.image_info_mipmap(resource.Id, mipmapId, out var width, out var height);
            Width = width;
            Height = height;
            Bytes =  Dll.image_get_mipmap(resource.Id, layerId, mipmapId, out var size);
            Size = size;
        }
    }
}
