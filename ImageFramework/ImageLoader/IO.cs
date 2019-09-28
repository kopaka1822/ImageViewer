 using System;
using System.Collections.Generic;
using System.Linq;
 using System.Runtime.InteropServices;
 using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;

namespace ImageFramework.ImageLoader
{
    public static class IO
    {
        /// <summary>
        /// tries to load the image file and returns a list with loaded images
        /// (one image file can contain multiple images with multiple faces with multiple mipmaps)
        /// </summary>
        /// <param name="file">filename</param>
        /// <returns></returns>
        public static Image LoadImage(string file)
        {
            var res = new Resource(file);
            Dll.image_info(res.Id, out var gliFormat, out var nLayer, out var nMipmaps);

            return new Image(res, file, nLayer, nMipmaps, new ImageFormat((GliFormat)gliFormat));
        }

        public static Image CreateImage(ImageFormat format, int width, int height, int layer, int mipmaps)
        {
            var res = new Resource((uint)format.GliFormat, width, height, layer, mipmaps);

            return new Image(res, "tmp", layer, mipmaps, format);
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
