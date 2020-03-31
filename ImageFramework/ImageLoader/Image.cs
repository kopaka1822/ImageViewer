using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.ImageLoader
{
    public class DllImageData : ImageData, IDisposable
    {
        // format of the source image (can be different form current format)
        public GliFormat OriginalFormat { get; }
        public string Filename { get; }

        // link to the dll resource id
        public Resource Resource { get; }

        public DllImageData(Resource resource, string filename, LayerMipmapCount lm, ImageFormat format, GliFormat originalFormat)
        : base(format, lm, GetSize(resource))
        {
            Resource = resource;
            Filename = filename;
            OriginalFormat = originalFormat;
        }

        private static Size3 GetSize(Resource res)
        {
            Dll.image_info_mipmap(res.Id, 0, out var width, out var height, out var depth);
            return new Size3(width, height, depth);
        }

        public override MipInfo GetMipmap(LayerMipmapSlice lm)
        {
            var res = new MipInfo();
            Dll.image_info_mipmap(Resource.Id, lm.Mipmap, out res.Size.X, out res.Size.Y, out res.Size.Z);
#if DEBUG
            var expected = Size.GetMip(lm.Mipmap);
            Debug.Assert(expected.Width == res.Size.Width);
            Debug.Assert(expected.Height == res.Size.Height);
            Debug.Assert(expected.Depth == res.Size.Depth);
#endif

            res.Bytes = Dll.image_get_mipmap(Resource.Id, lm.Layer, lm.Mipmap, out res.ByteSize);

            return res;
        }

        public void Dispose()
        {
            Resource.Dispose();
        }
    }
}
