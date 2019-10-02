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
        // complete filename
        public string Filename { get; }

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

        internal readonly ExportFormatModel ExportFormat;

        public ExportDescription(string filename, string extension, ExportModel model)
        {
            ExportFormat = model.Formats.First(f => f.Extension == extension);
            if(ExportFormat == null)
                throw new Exception("unsupported file extension: " + extension);

            Filename = filename + "." + extension;
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
