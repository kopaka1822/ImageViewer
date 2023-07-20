 using System;
using System.Collections.Generic;
 using System.Diagnostics;
 using System.Linq;
 using System.Runtime.CompilerServices;
 using System.Runtime.InteropServices;
using System.Text;
 using System.Threading;
 using System.Threading.Tasks;
 using ImageFramework.DirectX;
 using ImageFramework.Model;
 using ImageFramework.Model.Progress;
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
        public static DllImageData LoadImage(string file)
        {
            var res = new Resource(file);
            Dll.image_info(res.Id, out var gliFormat, out var originalFormat, out var nLayer, out var nMipmaps);

            return new DllImageData(res, file, new LayerMipmapCount(nLayer, nMipmaps), new ImageFormat((GliFormat)gliFormat), (GliFormat)originalFormat);
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

        /// <inheritdoc cref="LoadImageTexture(string,out ImageFramework.ImageLoader.GliFormat)"/>
        public static ITexture LoadImageTexture(string file)
        {
            return LoadImageTexture(file, out var dummy);
        }

        public class TexInfo
        {
            public ITexture Texture { get; set; }
            public GliFormat OriginalFormat { get; set; }
        }

        public static Task<TexInfo> LoadImageTextureAsync(string file, ProgressModel progress)
        {
            var task = Task.Run(() =>
            {
                var tex = LoadImageTexture(file, out var orig);
                return new TexInfo
                {
                    Texture = tex,
                    OriginalFormat = orig
                };
            });

            var cts = new CancellationTokenSource();
            progress.AddTask(task, cts, true);

            return task;
        }

        public static DllImageData CreateImage(ImageFormat format, Size3 size, LayerMipmapCount lm)
        {
            var res = new Resource((uint)format.GliFormat, size, lm);

            return new DllImageData(res, "tmp", lm, format, format.GliFormat);
        }

        public static void SaveImage(DllImageData image, string filename, string extension, GliFormat format, int quality = 0)
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

        // sets a global parameter for export
        public static void SetGlobalParameter(string name, int value)
        {
            Dll.set_global_parameter_i(name, value);
        }

        /// <summary>
        /// returns the shape of a numpy array
        /// </summary>
        [MethodImpl(MethodImplOptions.Synchronized)] // this method needs to be synchronized because the return value is not thread safe
        public static int[] NpyGetShape(string filename)
        {
            var ptr = Dll.npy_get_shape(filename, out var nDims);
            if (ptr == IntPtr.Zero)
                throw new Exception(Dll.GetError());

            // extract an integer array with nDims entries from IntPtr ptr
            var res = new int[nDims];
            Marshal.Copy(ptr, res, 0, (int)nDims);
            return res;
        }
    }
}
