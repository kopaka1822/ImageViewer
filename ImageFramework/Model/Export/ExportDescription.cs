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

        /// <summary>
        /// RGB colors will be multiplied by this value before exporting
        /// </summary>
        public float Multiplier { get; set; } = 1.0f;

        public string FullFilename => Filename + "." + Extension;

        // destination file format
        private GliFormat fileFormat;
        public GliFormat FileFormat
        {
            get => fileFormat;
            set
            {
                if(!ExportFormat.Formats.Contains(value))
                    throw new Exception($"format {value} is not supported for file extension {Extension}");

                fileFormat = value;
            }
        }

        internal ImageFormat StagingFormat
        {
            get
            {
                // type bigger than 8 bit => use float staging
                if(!FileFormat.IsAtMost8bit())
                    return new ImageFormat(GliFormat.RGBA32_SFLOAT);

                // determine staging format based on pixel data type
                var ldrMode = FileFormat.GetDataType();
                switch (ldrMode)
                {
                    case PixelDataType.Srgb:
                        return new ImageFormat(GliFormat.RGBA8_SRGB);
                    case PixelDataType.UNorm:
                        return new ImageFormat(GliFormat.RGBA8_UNORM);
                    case PixelDataType.SNorm:
                        return new ImageFormat(GliFormat.RGBA8_SNORM);
                    default: // all other formats (float, scaled, int) use float staging
                        return new ImageFormat(GliFormat.RGBA32_SFLOAT);
                }
            }
        }
        

        internal readonly ExportFormatModel ExportFormat;
        private readonly ExportModel exportModel;

        public ExportDescription(string filename, string extension, ExportModel model)
        {
            this.exportModel = model;
            ExportFormat = model.Formats.First(f => f.Extension == extension);
            if(ExportFormat == null)
                throw new Exception("unsupported file extension: " + extension);

            Filename = filename;
            Extension = extension;
            fileFormat = ExportFormat.Formats[0];
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
