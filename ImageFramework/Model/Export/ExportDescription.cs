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
                var ldrMode = exportModel.LdrExportMode;

                if (Extension == "ktx" || Extension == "dds")
                {
                    ldrMode = GetLdrMode(FileFormat); // overwrite ldr mode
                }

                if (!FileFormat.IsAtMost8bit() || ldrMode == ExportModel.LdrMode.Undefined)
                    return new ImageFormat(GliFormat.RGBA32_SFLOAT);

                switch (ldrMode)
                {
                    case ExportModel.LdrMode.Srgb:
                        return new ImageFormat(GliFormat.RGBA8_SRGB);
                    case ExportModel.LdrMode.UNorm:
                        return new ImageFormat(GliFormat.RGBA8_UNORM);
                    case ExportModel.LdrMode.SNorm:
                        return new ImageFormat(GliFormat.RGBA8_SNORM);
                    default:
                        Debug.Assert(false);
                        break;
                }

                return null;
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

        private ExportModel.LdrMode GetLdrMode(GliFormat format)
        {
            switch (format.GetDataType())
            {
                case PixelDataType.UNorm:
                    return ExportModel.LdrMode.UNorm;
                case PixelDataType.SNorm:
                    return ExportModel.LdrMode.SNorm;
                case PixelDataType.Srgb:
                    return ExportModel.LdrMode.Srgb;
            }
           
            // no ldr mode possible => use hdr staging format
            return ExportModel.LdrMode.Undefined;
        }
    }
}
