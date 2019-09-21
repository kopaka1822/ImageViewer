using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static List<Image> LoadImage(string file)
        {
            var res = new Resource(file);
            Dll.image_info(res.Id, out var internalFormat, out var externalFormat, out var openglType,
                out var nImages, out var nFaces, out var nMipmaps, out var isCompressed, out var isSrgb, out var gliFormat);

            /*var format = new ImageFormat((PixelFormat)externalFormat, (PixelType)openglType,
                (SizedInternalFormat)internalFormat, isSrgb, isCompressed, (GliFormat)gliFormat);*/
            // TODO correct format
            throw new NotImplementedException();
            var format = new ImageFormat();
            var images = new List<Image>(nImages);
            for (var curImage = 0; curImage < nImages; ++curImage)
            {
                images.Add(new Image(res, file, curImage, nFaces, nMipmaps, format));
            }

            return images;
        }

        public static void SavePng(string filename, int width, int height, int components, byte[] data)
        {
            if (!Dll.save_png(filename, width, height, components, data))
                throw new Exception("saving image failed: " + Dll.GetError());
        }

        public static void SaveBmp(string filename, int width, int height, int components, byte[] data)
        {
            if (!Dll.save_bmp(filename, width, height, components, data))
                throw new Exception("saving image failed: " + Dll.GetError());
        }

        public static void SaveHdr(string filename, int width, int height, int components, byte[] data)
        {
            if (!Dll.save_hdr(filename, width, height, components, data))
                throw new Exception("saving image failed: " + Dll.GetError());
        }

        public static void SaveJpg(string filename, int width, int height, int components, byte[] data, int quality)
        {
            if (!Dll.save_jpg(filename, width, height, components, data, quality))
                throw new Exception("saving image failed: " + Dll.GetError());
        }

        public static void SavePfm(string filename, int width, int height, int components, byte[] data)
        {
            if (!Dll.save_pfm(filename, width, height, components, data))
                throw new Exception("saving image failed: " + Dll.GetError());
        }

        public static void CreateStorage(/*TODO GLI format*/int format, int width, int height, int layer, int levels)
        {
            if (!Dll.create_storage((int)format, width, height, layer, levels))
                throw new Exception("create storage failed: " + Dll.GetError());
        }

        public static void StoreLevel(int layer, int level, byte[] data, UInt64 size)
        {
            if (!Dll.store_level(layer, level, data, size))
                throw new Exception($"store level failed (layer {layer}, level {level}): " + Dll.GetError());
        }

        public static void GetLevelSize(int level, out UInt64 size)
        {
            if (!Dll.get_level_size(level, out size))
                throw new Exception($"get level size failed (level {level}): " + Dll.GetError());
        }

        public static void SaveKtx(string filename)
        {
            if (!Dll.save_ktx(filename))
                throw new Exception("saving image failed: " + Dll.GetError());
        }

        public static void SaveDDS(string filename)
        {
            if (!Dll.save_dds(filename))
                throw new Exception("saving image failed: " + Dll.GetError());
        }
    }
}
