using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace TextureViewer.Models.Dialog
{
    public class ExportModel
    {
        public enum FileFormat
        {
            Png,
            Bmp,
            Hdr,
            Pfm
        }

        public ExportModel(string filename, PixelFormat pixelFromat, FileFormat format)
        {
            Filename = filename;
            PixelFormat = pixelFromat;
            Format = format;
            
            var supportedFormats = new List<PixelFormat>();
            switch (format)
            {
                case FileFormat.Png:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    supportedFormats.Add(PixelFormat.Rg);
                    supportedFormats.Add(PixelFormat.Rgb);
                    supportedFormats.Add(PixelFormat.Rgba);
                    break;
                case FileFormat.Bmp:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    supportedFormats.Add(PixelFormat.Rg);
                    supportedFormats.Add(PixelFormat.Rgb);
                    break;
                case FileFormat.Hdr:
                case FileFormat.Pfm:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    supportedFormats.Add(PixelFormat.Rgb);
                    break;
            }

            Debug.Assert(supportedFormats.Contains(pixelFromat));

            SupportedFormats = supportedFormats;
        }

        public string Filename { get; }

        public FileFormat Format;

        public int Layer { get; set; } = 0;

        public int Mipmap { get; set; } = 0;

        public PixelFormat PixelFormat { get; set; }

        public IReadOnlyList<PixelFormat> SupportedFormats { get; }

        public PixelType PixelType =>
            Format == FileFormat.Hdr || 
            Format == FileFormat.Pfm ? 
                PixelType.Float : PixelType.UnsignedByte;
    }
}
