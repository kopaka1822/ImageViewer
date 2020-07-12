using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;

namespace ImageFramework.DirectX
{
    public abstract class ImageData
    {
        public struct MipInfo
        {
            public IntPtr Bytes;
            public Size3 Size;
            public uint ByteSize;
        }

        protected ImageData(ImageFormat format, LayerMipmapCount layerMipmap, Size3 size)
        {
            Format = format;
            LayerMipmap = layerMipmap;
            Size = size;
        }

        public ImageFormat Format { get; }
        //public GliFormat OriginalFormat { get; set; }
        
        public LayerMipmapCount LayerMipmap { get; }
        public Size3 Size { get; }

        public bool Is3D => Size.Depth > 1;

        public abstract MipInfo GetMipmap(LayerMipmapSlice lm);
    }
}
