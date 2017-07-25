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

        public static int Test(int a, int b)
        {
            int id = open("hello");

            release(id);
            return 0;
        }
    }
}
