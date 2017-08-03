using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer
{
    public static class ImageLoader
    {
        private const string DLLFilePath = @"ImageLoader.dll";

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int open(string filename);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void release(int id);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void image_info(int id, out uint openglInternalFormat, out uint openglExternalFormat,
            out uint openglType, out int nImages, out int nFaces, out int nMipmaps, out bool isCompressed);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void image_info_mipmap(int id, int mipmap, out int width, out int height);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr image_get_mipmap(int id, int image, int face, int mipmap, out uint size);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public class Resource
        {
            public int Id { get; }

            public Resource(string file)
            {
                Id = open(file);
                if (Id == 0)
                    throw new Exception("could not open " + file);
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

        public class Image
        {
            public readonly uint OpenglInternalFormat;
            public readonly uint OpenglExternalFormat;
            public readonly uint OpenglType;
            public readonly bool IsCompressed;
            public readonly List<Face> Layers;
            public readonly string Filename;

            public Image(Resource resource, string filename, uint internalFormat, uint externalFormat,
                uint type, int curImage, int nFaces, int nMipmaps, bool isCompressed)
            {
                Filename = filename;
                OpenglExternalFormat = externalFormat;
                OpenglInternalFormat = internalFormat;
                OpenglType = type;
                IsCompressed = isCompressed;
                // load relevant information

                Layers = new List<Face>(nFaces);
                for (int curLayer = 0; curLayer < nFaces; ++curLayer)
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

            public int GetNumMipmaps()
            {
                if (Layers.Count > 0)
                    return Layers[0].Mipmaps.Count;
                return 0;
            }

            public bool IsGrayscale()
            {
                // TODO
                return false;
                /*
                switch (OpenglExternalFormat)
                {
                    case OpenGL.GL_RED:
                    case OpenGL.GL_RED_INTEGER:
                    case OpenGL.GL_DEPTH_COMPONENT:
                    case OpenGL.GL_LUMINANCE:
                    case OpenGL.GL_ALPHA:
                    case OpenGL.GL_LUMINANCE_ALPHA:
                        return true;
                }
                return false;*/
            }
        }

        public static List<Image> LoadImage(string file)
        {
            Resource res = new Resource(file);
            uint internalFormat;
            uint externalFormat;
            uint openglType;
            int nImages;
            int nFaces;
            int nMipmaps;
            bool isCompressed;
            image_info(res.Id, out internalFormat, out externalFormat, out openglType,
                out nImages, out nFaces, out nMipmaps, out isCompressed);
            List<Image> images = new List<Image>(nImages);
            for (int curImage = 0; curImage < nImages; ++curImage)
            {
                images.Add(new Image(res, file, internalFormat, externalFormat, openglType,
                    curImage, nFaces, nMipmaps, isCompressed));
            }

            return images;
        }

        public static int Test(int a, int b)
        {
            int id = open("hello");

            release(id);
            return 0;
        }
    }
}
