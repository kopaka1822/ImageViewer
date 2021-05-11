using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;

namespace ImageFramework.Model.Export
{
    public class ExportFormatModel
    {
        public string Extension { get; }

        public IReadOnlyList<GliFormat> Formats { get; }

        public ExportFormatModel(string extension)
        {
            Formats = IO.GetExportFormats(extension);
            Debug.Assert(Formats.Count > 0);

            Extension = extension;
        }

        public bool SupportsQuality(GliFormat format)
        {
            switch (Extension)
            {
                case "jpg": return true;
                case "ktx":
                case "dds":
                    return format.IsCompressed();
                case "ktx2":
                    if (format.IsCompressed()) return true; // compressed formats support compression quality level
                    if (!format.Is8Bit()) return false; // only 8 bit per pixel formats
                    //return format.IsCompressed(); // TODO support the supercompressed stuff later on
                    // if (!format.IsCompressed() && !format.Is8Bit()) return false;
                    var type = format.GetDataType();
                    if (type == PixelDataType.UInt || type == PixelDataType.SInt) return false;
                    if (type == PixelDataType.UScaled || type == PixelDataType.SScaled) return false;
                    if (type == PixelDataType.SNorm) return false;
                    return true; // UNorm, Srgb, UFloat, SFloat
                default: return false;
            }
        }
    }
}
