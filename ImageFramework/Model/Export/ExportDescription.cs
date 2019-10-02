using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;

namespace ImageFramework.Model.Export
{
    public class ExportDescription
    {
        public string Filename { get; }

        public string Extension { get; }

        public string FullFilename => Filename + "." + Extension;

        // destination file format
        private GliFormat fileFormat;
        public GliFormat FileFormat
        {
            get => fileFormat;
            set
            {
                Debug.Assert(ExportFormat.Formats.Contains(value));
                fileFormat = value;
            }
        }

        internal ImageFormat StagingFormat { get; }

        internal readonly ExportFormatModel ExportFormat;

        public ExportDescription(string filename, string extension, ExportModel model)
        {
            ExportFormat = model.Formats.First(f => f.Extension == extension);
            if(ExportFormat == null)
                throw new Exception("unsupported file extension: " + extension);

            Filename = filename;
            Extension = extension;
            fileFormat = ExportFormat.Formats[0];

            // set staging format
            if (extension == "png" || extension == "jpg" || extension == "bmp")
            {
                switch (model.LdrExportMode)
                {
                    case ExportModel.LdrMode.Srgb:
                        StagingFormat = new ImageFormat(GliFormat.RGBA8_SRGB_PACK8);
                        break;
                    case ExportModel.LdrMode.UNorm:
                        StagingFormat = new ImageFormat(GliFormat.RGBA8_UNORM_PACK8);
                        break;
                    case ExportModel.LdrMode.SNorm:
                        StagingFormat = new ImageFormat(GliFormat.RGBA8_SNORM_PACK8);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
            else
            {
                StagingFormat = new ImageFormat(GliFormat.RGBA32_SFLOAT_PACK32);
            }
        }

        /// <summary>
        /// tries to set the export file format
        /// </summary>
        /// <param name="format"></param>
        /// <returns>true if the format is supported</returns>
        public bool TrySetFormat(GliFormat format)
        {
            if (!ExportFormat.Formats.Contains(format)) return false;
            FileFormat = format;
            return true;
        }
    }
}
