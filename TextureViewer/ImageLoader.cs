using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Utility;

namespace TextureViewer
{
    public static class ImageLoader
    {
        private const string DllFilePath = @"ImageLoader.dll";

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int open(string filename);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void release(int id);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void image_info(int id, out uint openglInternalFormat, 
            out uint openglExternalFormat, out uint openglType, out int nImages, out int nFaces, 
            out int nMipmaps, out bool isCompressed, out bool isSrgb);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void image_info_mipmap(int id, int mipmap, out int width, out int height);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr image_get_mipmap(int id, int image, int face, int mipmap, out uint size);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr get_error(out int length);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool save_png(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool save_bmp(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool save_hdr(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool save_pfm(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool save_jpg(string filename, int width, int height, int components, byte[] data, int quality);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool save_2d_ktx(string filename, int format, int width, int height, int levels, byte[] data, UInt64 size);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void gli_to_opengl_format(int gliFormat, out int glInternal, out int glExternal, out int glType, out bool isCompressed, out bool isSrgb);

        private static string GetError()
        {
            var ptr = get_error(out var length);
            return ptr.Equals(IntPtr.Zero) ? "" : Marshal.PtrToStringAnsi(ptr, length);
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public class Resource
        {
            public int Id { get; }

            public Resource(string file)
            {
                Id = open(file);
                if (Id == 0)
                    throw new Exception("error in " + file + ": " + GetError());
            }

            ~Resource()
            {
                if (Id != 0)
                    release(Id);
            }
        }

        public class Mipmap
        {
            public readonly int Width;
            public readonly int Height;
            public readonly IntPtr Bytes;
            public readonly uint Size;

            public Mipmap(Resource resource, int imageId, int layerId, int mipmapId)
            {
                image_info_mipmap(resource.Id, mipmapId, out Width, out Height);

                IntPtr ptr = image_get_mipmap(resource.Id, imageId, layerId, mipmapId, out Size);
                Bytes = Marshal.AllocHGlobal((int)Size);

                CopyMemory(Bytes, ptr, Size);
            }

            ~Mipmap()
            {
                Marshal.FreeHGlobal(Bytes);
            }
        }

        public class Face
        {
            public readonly List<Mipmap> Mipmaps;

            public Face(Resource resource, int imageId, int layerId, int nMipmaps)
            {
                Mipmaps = new List<Mipmap>(nMipmaps);
                for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
                {
                    Mipmaps.Add(new Mipmap(resource, imageId, layerId, curMipmap));
                }
            }
        }

        public struct ImageFormat
        {
            public PixelFormat ExternalFormat { get; set; }
            public PixelFormat Format => IsCompressed ? (PixelFormat)InternalFormat : ExternalFormat;
            public PixelType Type { get; set; }
            public bool IsSrgb { get; set; }
            public bool IsCompressed { get; set; }
            public SizedInternalFormat InternalFormat { get; set; }

            public ImageFormat(PixelFormat format, PixelType type, bool isSrgb)
            {
                ExternalFormat = format;
                Type = type;
                IsSrgb = isSrgb;
                IsCompressed = false;
                InternalFormat = (SizedInternalFormat)0;
            }

            public ImageFormat(PixelFormat externalFormat, PixelType type, SizedInternalFormat internalFormat, bool isSrgb, bool isCompressed) : this()
            {
                ExternalFormat = externalFormat;
                Type = type;
                InternalFormat = internalFormat;
                IsSrgb = isSrgb;
                IsCompressed = isCompressed;
            }

            public ImageFormat(GliFormat format)
            {
                gli_to_opengl_format((int)format, out var intForm, out var extForm, out var pt, out var compressed, out var srgb);
                ExternalFormat = (PixelFormat)extForm;
                Type = (PixelType)pt;
                InternalFormat = (SizedInternalFormat)intForm;
                IsSrgb = srgb;
                IsCompressed = compressed;
            }

            public bool Equals(ImageFormat other)
            {
                if (IsCompressed != other.IsCompressed) return false;
                if (IsSrgb != other.IsSrgb) return false;
                if (Format != other.Format) return false;
                // type is only important is the format is uncompressed
                if (!IsCompressed && Type != other.Type) return false;
                return true;
            }
        }

        public class Image
        {
            public readonly ImageFormat Format;
            public readonly List<Face> Layers;
            public readonly string Filename;

            public Image(Resource resource, string filename, int curImage, int nFaces, int nMipmaps, ImageFormat format)
            {
                Filename = filename;
                Format = format;
                // load relevant information

                Layers = new List<Face>(nFaces);
                for (var curLayer = 0; curLayer < nFaces; ++curLayer)
                {
                    Layers.Add(new Face(resource, curImage, curLayer, nMipmaps));
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
                switch (Format.ExternalFormat)
                {
                    case PixelFormat.UnsignedShort:
                    case PixelFormat.UnsignedInt:
                    case PixelFormat.ColorIndex:
                    case PixelFormat.StencilIndex:
                    case PixelFormat.DepthComponent:
                    case PixelFormat.Red:
                    case PixelFormat.Green:
                    case PixelFormat.Blue:
                    case PixelFormat.Alpha:
                    case PixelFormat.Luminance:
                    case PixelFormat.LuminanceAlpha:
                    case PixelFormat.RedInteger:
                    case PixelFormat.GreenInteger:
                    case PixelFormat.BlueInteger:
                    case PixelFormat.AlphaInteger:
                        return true;
                }
                return false;
            }

            /// <summary>
            /// checks for availability of the alpha components
            /// </summary>
            /// <returns>true if image has an alpha component</returns>
            public bool HasAlpha()
            {
                switch (Format.ExternalFormat)
                {
                    case PixelFormat.Rgba:
                    case PixelFormat.LuminanceAlpha:
                    case PixelFormat.AbgrExt:
                    case PixelFormat.CmykaExt:
                    case PixelFormat.Bgra:
                    case PixelFormat.Alpha16IccSgix:
                    case PixelFormat.Luminance16Alpha8IccSgix:
                    case PixelFormat.RgbaInteger:
                    case PixelFormat.BgraInteger:
                        return true;
                }
                return false;
            }

            /// <summary>
            /// checks for high dimension range
            /// </summary>
            /// <returns>true if the image type is bigger than byte (range > [0-255])</returns>
            public bool IsHdr()
            {
                switch (Format.Type)
                {
                    case PixelType.Short:
                    case PixelType.UnsignedShort:
                    case PixelType.Int:
                    case PixelType.UnsignedInt:
                    case PixelType.Float:
                    case PixelType.HalfFloat:
                    case PixelType.UnsignedShort4444:
                    case PixelType.UnsignedShort5551:
                    case PixelType.UnsignedInt8888:
                    case PixelType.UnsignedInt1010102:
                    case PixelType.UnsignedShort565:
                    case PixelType.UnsignedShort565Reversed:
                    case PixelType.UnsignedShort4444Reversed:
                    case PixelType.UnsignedShort1555Reversed:
                    case PixelType.UnsignedInt8888Reversed:
                    case PixelType.UnsignedInt2101010Reversed:
                    case PixelType.UnsignedInt248:
                    case PixelType.UnsignedInt10F11F11FRev:
                    case PixelType.UnsignedInt5999Rev:
                    case PixelType.Float32UnsignedInt248Rev:
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// tries to load the image file and returns a list with loaded images
        /// (one image file can contain multiple images with multiple faces with multiple mipmaps)
        /// </summary>
        /// <param name="file">filename</param>
        /// <returns></returns>
        public static List<Image> LoadImage(string file)
        {
            var res = new Resource(file);
            image_info(res.Id, out var internalFormat, out var externalFormat, out var openglType,
                out var nImages, out var nFaces, out var nMipmaps, out var isCompressed, out var isSrgb);

            var format = new ImageFormat((PixelFormat)externalFormat, (PixelType)openglType, (SizedInternalFormat)internalFormat, isSrgb, isCompressed);
            var images = new List<Image>(nImages);
            for (var curImage = 0; curImage < nImages; ++curImage)
            {
                images.Add(new Image(res, file, curImage, nFaces, nMipmaps, format));
            }

            return images;
        }

        public static void SavePng(string filename, int width, int height, int components, byte[] data)
        {
            if(!save_png(filename, width, height, components, data))
                throw new Exception("saving image failed: " + GetError());
        }

        public static void SaveBmp(string filename, int width, int height, int components, byte[] data)
        {
            if (!save_bmp(filename, width, height, components, data))
                throw new Exception("saving image failed: " + GetError());
        }

        public static void SaveHdr(string filename, int width, int height, int components, byte[] data)
        {
            if (!save_hdr(filename, width, height, components, data))
                throw new Exception("saving image failed: " + GetError());
        }

        public static void SaveJpg(string filename, int width, int height, int components, byte[] data, int quality)
        {
            if (!save_jpg(filename, width, height, components, data, quality))
                throw new Exception("saving image failed: " + GetError());
        }

        public static void SavePfm(string filename, int width, int height, int components, byte[] data)
        {
            if (!save_pfm(filename, width, height, components, data))
                throw new Exception("saving image failed: " + GetError());
        }

        public static void SaveKtx2D(string filename, GliFormat format, int width, int height, int levels, byte[] data, UInt64 size)
        {
            if (!save_2d_ktx(filename, (int)format, width, height, levels, data, size))
                throw new Exception("saving image failed: " + GetError());
        }
    }
}
