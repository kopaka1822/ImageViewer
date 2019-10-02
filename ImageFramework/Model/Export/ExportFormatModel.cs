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

        internal ImageFormat StagingFormat { get; }

        public ExportFormatModel(string extension)
        {
            Formats = IO.GetExportFormats(extension);
            Debug.Assert(Formats.Count > 0);

            Extension = extension;
            StagingFormat = IO.GetStagingFormat(extension);
        }
    }
}
