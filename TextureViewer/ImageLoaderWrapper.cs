using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer
{
    public static class ImageLoaderWrapper
    {
        private const string DLLFilePath = @"E:\git\TextureViewer\ImageLoader\Bin\ImageLoader.dll";

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern int open(string filename);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void release(int id);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void image_info(int id, out int nComponents, out int componentSize, out bool isIntegerFormat, out int nLayers, out int nMipmaps);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void image_info_mipmap(int id, int mipmap, out int width, out int height, out uint size);

        [DllImport(DLLFilePath, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr image_get_mipmap(int id, int layer, int mipmap);

        public class Resource
        {
            public int Id { get; }

            public Resource(string file)
            {
                Id = open(file);
                if(Id == 0)
                    throw new Exception("could not open " + file);
            }

            ~Resource()
            {
                if(Id != 0)
                    release(Id);
            }
        }

        public class Mipmap
        {
            public readonly int Width;
            public readonly int Height;
            public readonly byte[] Bytes;

            public Mipmap(Resource resource, int layerId, int mipmapId)
            {
                uint size;
                image_info_mipmap(resource.Id, mipmapId, out Width, out Height, out size);
                Bytes = new byte[size];

                IntPtr ptr = image_get_mipmap(resource.Id, layerId, mipmapId);
                Marshal.Copy(ptr, Bytes, 0, (int)size);
            }
        }

        public class Layer
        {
            public readonly List<Mipmap> Mipmaps;

            public Layer(Resource resource, int layerId, int nMipmaps)
            {
                Mipmaps = new List<Mipmap>(nMipmaps);
                for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
                {
                    Mipmaps.Add(new Mipmap(resource, layerId, curMipmap));
                }
            }
        }

        public class Image
        {
            public readonly int NumComponents;
            public readonly int ComponentSize;
            public readonly bool IsIntegerFormat;
            public readonly List<Layer> Layers;
            public readonly string Filename;

            public Image(Resource resource, string filename)
            {
                this.Filename = filename;
                // load relevant information
                int nLayer;
                int nMipmaps;
                image_info(resource.Id, out NumComponents, out ComponentSize, out IsIntegerFormat, out nLayer, out nMipmaps);
                Layers = new List<Layer>(nLayer);
                for (int curLayer = 0; curLayer < nLayer; ++curLayer)
                {
                    Layers.Add(new Layer(resource, curLayer, nMipmaps));
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
        }

        public static Image LoadImage(string file)
        {
            return new Image(new Resource(file), file);
        }

        public static int Test(int a, int b)
        {
            int id = open("hello");

            release(id);
            return 0;
        }
    }
}
