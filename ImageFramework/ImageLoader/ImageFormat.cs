using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;

namespace ImageFramework.ImageLoader
{
    public class ImageFormat
    {
        public GliFormat GliFormat { get; }
        public SharpDX.DXGI.Format DxgiFormat { get; }
        public uint PixelSize { get; }

        public ImageFormat(GliFormat format)
        {
            GliFormat = format;
            switch (format)
            {
                case GliFormat.RGBA32_SFLOAT:
                    DxgiFormat = Format.R32G32B32A32_Float;
                    PixelSize = 4 * 4;
                    break;
                case GliFormat.RGBA8_SRGB:
                    DxgiFormat = Format.R8G8B8A8_UNorm_SRgb;
                    PixelSize = 4;
                    break;
                case GliFormat.RGBA8_UNORM:
                    DxgiFormat = Format.R8G8B8A8_UNorm;
                    PixelSize = 4;
                    break;
                case GliFormat.RGBA8_SNORM:
                    DxgiFormat = Format.R8G8B8A8_SNorm; 
                    PixelSize = 4;
                    break;
                case GliFormat.BGRA8_SRGB:
                    DxgiFormat = Format.B8G8R8A8_UNorm_SRgb;
                    PixelSize = 4;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        public ImageFormat(SharpDX.DXGI.Format format)
        {
            DxgiFormat = format;
            switch (format)
            {
                case Format.R32G32B32A32_Float:
                    GliFormat = GliFormat.RGBA32_SFLOAT;
                    PixelSize = 4 * 4;
                    break;
                case Format.R8G8B8A8_UNorm_SRgb:
                    GliFormat = GliFormat.RGBA8_SRGB;
                    PixelSize = 4;
                    break;
                case Format.R8G8B8A8_UNorm:
                    GliFormat = GliFormat.RGBA8_UNORM;
                    PixelSize = 4;
                    break;
                case Format.R8G8B8A8_SNorm:
                    GliFormat = GliFormat.RGBA8_SNORM;
                    PixelSize = 4;
                    break;
                case Format.B8G8R8A8_UNorm_SRgb:
                    GliFormat = GliFormat.BGRA8_SRGB;
                    PixelSize = 4;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// indicates if this format may be used internally
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool IsSupported(Format format)
        {
            return IO.SupportedFormats.Contains(format);
        }
    }
}
