 using System;
using System.Collections.Generic;
using System.Linq;
 using System.Runtime.InteropServices;
 using System.Text;
using System.Threading.Tasks;
 using ImageFramework.DirectX;
 using ImageFramework.Utility;
 using SharpDX.DXGI;

namespace ImageFramework.ImageLoader
{
    public static class IO
    {
        /// <summary>
        /// list of formats that are supported internally for images. This does not include formats that
        /// may be used for save image
        /// </summary>
        public static readonly List<SharpDX.DXGI.Format> SupportedFormats = new List<Format>
        {
            Format.R32G32B32A32_Float,
            Format.R8G8B8A8_UNorm_SRgb,
            Format.R8G8B8A8_UNorm,
            Format.R8G8B8A8_SNorm,
            // extra format for thumbnails
            Format.B8G8R8A8_UNorm_SRgb
        };

        /// <summary>
        /// tries to load the image file and returns a list with loaded images
        /// (one image file can contain multiple images with multiple faces with multiple mipmaps)
        /// </summary>
        /// <param name="file">filename</param>
        /// <returns></returns>
        public static Image LoadImage(string file)
        {
            var res = new Resource(file);
            Dll.image_info(res.Id, out var gliFormat, out var originalFormat, out var nLayer, out var nMipmaps);

            return new Image(res, file, nLayer, nMipmaps, new ImageFormat((GliFormat)gliFormat), (GliFormat)originalFormat);
        }

        /// <summary>
        /// loads image into the correct texture type
        /// </summary>
        public static ITexture LoadImageTexture(string file, out GliFormat originalFormat)
        {
            using (var img = LoadImage(file))
            {
                originalFormat = img.OriginalFormat;

                if(img.Is3D) return new Texture3D(img);
                return new TextureArray2D(img);
            }
        }

        public static Image CreateImage(ImageFormat format, Size3 size, int layer, int mipmaps)
        {
            var res = new Resource((uint)format.GliFormat, size, layer, mipmaps);

            return new Image(res, "tmp", layer, mipmaps, format, format.GliFormat);
        }

        public static void SaveImage(Image image, string filename, string extension, GliFormat format, int quality = 0)
        {
            if(!Dll.image_save(image.Resource.Id, filename, extension, (uint) format, quality))
                throw new Exception(Dll.GetError());
        }

        public static List<GliFormat> GetExportFormats(string extension)
        {
            var ptr = Dll.get_export_formats(extension, out var nFormats);
            if(ptr == IntPtr.Zero)
                throw new Exception(Dll.GetError());

            var rawArray = new int[nFormats];
            Marshal.Copy(ptr, rawArray, 0, nFormats);

            var res = new List<GliFormat>(nFormats);
            res.AddRange(rawArray.Select(i => (GliFormat) i));

            return res;
        }

        
    }
}
