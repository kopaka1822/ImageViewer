using ImageFramework.ImageLoader;
using ImageViewer.UtilityEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Commands.Export
{
    // TODO use
    internal class SharedExportData
    {
        public static SharedExportData Instance => instance ?? (instance = new SharedExportData());

        public GliFormat? ExportFormat { get; } = null;

        public PathManager Path { get; }

        private static SharedExportData instance = null;

        public SharedExportData()
        {
            Path = new PathManager();
        }
    }
}
