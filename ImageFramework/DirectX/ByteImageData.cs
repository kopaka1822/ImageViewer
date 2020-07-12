using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;

namespace ImageFramework.DirectX
{
    public class ByteImageData : ImageData
    {
        private GCHandle handle;
        private readonly IntPtr ptr;

        public ByteImageData(byte[] data, LayerMipmapCount lm, Size3 size, ImageFormat format)
        : base(format, lm, size)
        {
            handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            ptr = handle.AddrOfPinnedObject();
        }

        public override MipInfo GetMipmap(LayerMipmapSlice lm)
        {
            // calc offset
            uint mipOffset = 0;
            // offset for previous layers
            for (int i = 0; i < lm.Mipmap; ++i)
            {
                // add mipmap size * layers
                mipOffset += (uint) Size.GetMip(i).Product * Format.PixelSize * (uint)LayerMipmap.Layers;
            }
            // offset from current layer
            var res = new MipInfo();
            res.Size = Size.GetMip(lm.Mipmap);
            res.ByteSize = (uint)res.Size.Product * Format.PixelSize;
            mipOffset += res.ByteSize * (uint) lm.Layer;
            res.Bytes = ptr + (int)mipOffset;
            return res;
        }

        ~ByteImageData()
        {
            handle.Free();
        }
    }
}
