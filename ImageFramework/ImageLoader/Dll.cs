using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    internal static class Dll
    {
        public const string DllFilePath = @"DxImageLoader.dll";

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern int open(string filename);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void release(int id);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void image_info(int id, out uint format,
            out int nLayer, out int nMipmaps, out bool isSrgb, out bool hasAlpha);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void image_info_mipmap(int id, int mipmap, out int width, out int height);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr image_get_mipmap(int id, int layer, int mipmap, out uint size);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr get_error(out int length);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_png(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_bmp(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_hdr(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_pfm(string filename, int width, int height, int components, byte[] data);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_jpg(string filename, int width, int height, int components, byte[] data, int quality);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool create_storage(int format, int width, int height, int layer, int levels);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool store_level(int layer, int level, byte[] data, UInt64 size);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool get_level_size(int level, out UInt64 size);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_ktx(string filename);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool save_dds(string filename);

        [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
        public static extern void gli_to_opengl_format(int gliFormat, out int glInternal, out int glExternal, out int glType, out bool isCompressed, out bool isSrgb);

        public static string GetError()
        {
            var ptr = get_error(out var length);
            return ptr.Equals(IntPtr.Zero) ? "" : Marshal.PtrToStringAnsi(ptr, length);
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }
}
