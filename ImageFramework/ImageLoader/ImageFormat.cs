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

        public ImageFormat(GliFormat format)
        {
            GliFormat = format;
            switch (format)
            {
                case GliFormat.RGBA32_SFLOAT_PACK32:
                    DxgiFormat = Format.R32G32B32A32_Float;
                    break;
                case GliFormat.RGBA8_SRGB_PACK8:
                    DxgiFormat = Format.R8G8B8A8_UNorm_SRgb;
                    break;
                case GliFormat.RGBA8_UNORM_PACK8:
                    DxgiFormat = Format.R8G8B8A8_UNorm;
                    break;
                case GliFormat.RGBA8_SNORM_PACK8:
                    DxgiFormat = Format.R8G8B8A8_SNorm;
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
                    GliFormat = GliFormat.RGBA32_SFLOAT_PACK32;
                    break;
                case Format.R8G8B8A8_UNorm_SRgb:
                    GliFormat = GliFormat.RGBA8_SRGB_PACK8;
                    break;
                case Format.R8G8B8A8_UNorm:
                    GliFormat = GliFormat.RGBA8_UNORM_PACK8;
                    break;
                case Format.R8G8B8A8_SNorm:
                    GliFormat = GliFormat.RGBA8_SNORM_PACK8;
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
