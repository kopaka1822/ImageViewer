using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Export
{
    public class ExportDescription
    {
        public string Filename { get; }

        public string Extension { get; }

        public ITexture Texture { get; }

        public ITexture Overlay { get; set; }

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

        private int quality = 100;

        /// <summary>
        /// image quality for compressed formats and jpg
        /// Range [1, 100] => [QualityMin, QualityMax]
        /// </summary>
        public int Quality
        {
            get => quality;
            set
            {
                Debug.Assert(quality <= QualityMax);
                Debug.Assert(quality >= QualityMin);
                quality = value;
            }
        }

        public static int QualityMin = 1;
        public static int QualityMax = 100;

        public bool UseCropping { get; set; }

        private Float3 cropStartf = Float3.Zero;

        /// <summary>
        /// crop start in relative coordinates [0, 1]
        /// CropStart.ToPixel is the first included pixel
        /// </summary>
        public Float3 CropStart
        {
            get => cropStartf;
            set
            {
                Debug.Assert((value >= Float3.Zero).AllTrue());
                Debug.Assert((value <= Float3.One).AllTrue());
                cropStartf = value;
            }
        }

        private Float3 cropEndf = Float3.One;

        /// <summary>
        /// crop end in relative coordinates [0, 1]
        /// CropEnd.ToPixel is the last included pixel.
        /// CropStart == CropEnd => exactly one pixel will be exported
        /// </summary>
        public Float3 CropEnd
        {
            get => cropEndf;
            set
            {
                Debug.Assert((value >= Float3.Zero).AllTrue());
                Debug.Assert((value <= Float3.One).AllTrue());
                cropEndf = value;
            }
        }

        private int layer = -1;
        // layer to export. -1 means all layers
        public int Layer
        {
            get => layer;
            set
            {
                Debug.Assert(layer >= -1);
                Debug.Assert(layer < Texture.NumLayers);
                layer = value;
            }
        }

        private int mipmap = -1;
        // mipmap to export. -1 means all mipmaps
        public int Mipmap
        {
            get => mipmap;
            set
            {
                Debug.Assert(mipmap >= -1);
                Debug.Assert(mipmap < Texture.NumMipmaps);
                mipmap = value;
            }
        }

        internal readonly ExportFormatModel ExportFormat;

        public ExportDescription(ITexture texture, string filename, string extension)
        {
            Debug.Assert(texture != null);
            Texture = texture;
            ExportFormat = GetExportFormat(extension);
            if(ExportFormat == null)
                throw new Exception("unsupported file extension: " + extension);

            Filename = filename;
            Extension = extension;
            fileFormat = ExportFormat.Formats[0];
        }

        /// <summary>
        /// verifies if the properties have valid ranges
        /// </summary>
        public void Verify()
        {
            if(Mipmap >= Texture.NumMipmaps || Mipmap < -1)
                throw new Exception("export mipmap out of range");
            if(Layer >= Texture.NumLayers || Layer < -1)
                throw new Exception("export layer out of range");

            if (Overlay != null)
            {
                if (!Texture.HasSameDimensions(Overlay))
                    throw new Exception("export overlay size mismatch");
            }
        }

        public void GetCropRect(out Size3 start, out Size3 end)
        {
            var mipDim = Texture.Size.GetMip(Math.Max(Mipmap, 0));
            start = CropStart.ToPixels(mipDim);
            end = CropEnd.ToPixels(mipDim);

            if ((start < Size3.Zero).AnyTrue() || (start >= mipDim).AnyTrue())
                throw new Exception("export crop start out of range: " + start);

            if ((end < Size3.Zero).AnyTrue() || (end >= mipDim).AnyTrue())
                throw new Exception("export crop end out of range: " + end);

            // end >= max
            if ((start > end).AnyTrue())
                throw new Exception("export crop start must be smaller or equal to crop end");
        }

        /// <summary>
        /// indicates if the texture must be realigned according to the used format (compressed formats need alignment)
        /// </summary>
        public bool RequiresAlignment
        {
            get
            {
                if (!FileFormat.IsCompressed()) return false;
                var mipDim = Texture.Size.GetMip(Math.Max(Mipmap, 0));
                if (mipDim.Width % FileFormat.GetAlignmentX() != 0)
                    return true;
                if (mipDim.Height % FileFormat.GetAlignmentY() != 0)
                    return true;
                return false;
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

        private static List<ExportFormatModel> s_exportFormatModels;
        public static ExportFormatModel GetExportFormat(string extension)
        {
            return Formats.First(f => f.Extension == extension);
        }

        public static IReadOnlyList<ExportFormatModel> Formats
        {
            get
            {
                if (s_exportFormatModels == null)
                {
                    var formats = new List<ExportFormatModel>();
                    formats.Add(new ExportFormatModel("png"));
                    formats.Add(new ExportFormatModel("jpg"));
                    formats.Add(new ExportFormatModel("bmp"));
                    formats.Add(new ExportFormatModel("hdr"));
                    formats.Add(new ExportFormatModel("pfm"));
                    formats.Add(new ExportFormatModel("dds"));
                    formats.Add(new ExportFormatModel("ktx"));
                    s_exportFormatModels = formats;
                }

                return s_exportFormatModels;
            }
        }
    }
}
