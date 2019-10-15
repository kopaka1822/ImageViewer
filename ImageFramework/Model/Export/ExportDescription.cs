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

                if (!Is8bitStaging(FileFormat))
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

        private bool Is8bitStaging(GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RGBA4_UNORM:
                case GliFormat.BGRA4_UNORM:
                case GliFormat.R5G6B5_UNORM:
                case GliFormat.B5G6R5_UNORM:
                case GliFormat.RGB5A1_UNORM:
                case GliFormat.BGR5A1_UNORM:
                case GliFormat.A1RGB5_UNORM:
                case GliFormat.R8_UNORM:
                case GliFormat.R8_SNORM:
                case GliFormat.R8_USCALED:         
                case GliFormat.R8_SSCALED:
                case GliFormat.R8_UINT:
                case GliFormat.R8_SINT:            
                case GliFormat.R8_SRGB:                   
                case GliFormat.RG8_UNORM:                  
                case GliFormat.RG8_SNORM:                 
                case GliFormat.RG8_USCALED:
                case GliFormat.RG8_SSCALED:
                case GliFormat.RG8_UINT:                    
                case GliFormat.RG8_SINT:                    
                case GliFormat.RG8_SRGB:                    
                case GliFormat.RGB8_UNORM:                    
                case GliFormat.RGB8_SNORM:                    
                case GliFormat.RGB8_USCALED:                    
                case GliFormat.RGB8_SSCALED:                    
                case GliFormat.RGB8_UINT:                    
                case GliFormat.RGB8_SINT:                    
                case GliFormat.RGB8_SRGB:                    
                case GliFormat.BGR8_UNORM:                    
                case GliFormat.BGR8_SNORM:                    
                case GliFormat.BGR8_USCALED:                    
                case GliFormat.BGR8_SSCALED:                    
                case GliFormat.BGR8_UINT:                    
                case GliFormat.BGR8_SINT:                    
                case GliFormat.BGR8_SRGB:                   
                case GliFormat.RGBA8_UNORM:               
                case GliFormat.RGBA8_SNORM:                    
                case GliFormat.RGBA8_USCALED:                    
                case GliFormat.RGBA8_SSCALED:                    
                case GliFormat.RGBA8_UINT:                    
                case GliFormat.RGBA8_SINT:                    
                case GliFormat.RGBA8_SRGB:                    
                case GliFormat.BGRA8_UNORM:                    
                case GliFormat.BGRA8_SNORM:                    
                case GliFormat.BGRA8_USCALED:                    
                case GliFormat.BGRA8_SSCALED:                    
                case GliFormat.BGRA8_UINT:                    
                case GliFormat.BGRA8_SINT:                    
                case GliFormat.BGRA8_SRGB:                    
                case GliFormat.RGBA8_UNORM_PACK32:                    
                case GliFormat.RGBA8_SNORM_PACK32:                    
                case GliFormat.RGBA8_USCALED_PACK32:                    
                case GliFormat.RGBA8_SSCALED_PACK32:                    
                case GliFormat.RGBA8_UINT_PACK32:                    
                case GliFormat.RGBA8_SINT_PACK32:                    
                case GliFormat.RGBA8_SRGB_PACK32:
                case GliFormat.L8_UNORM:
                case GliFormat.A8_UNORM:
                case GliFormat.LA8_UNORM:
                case GliFormat.BGR8_UNORM_PACK32:
                case GliFormat.BGR8_SRGB_PACK32:
                case GliFormat.RG3B2_UNORM:
                    return true;

                case GliFormat.RGB10A2_UNORM:                    
                case GliFormat.RGB10A2_SNORM:                    
                case GliFormat.RGB10A2_USCALED:                    
                case GliFormat.RGB10A2_SSCALED:                    
                case GliFormat.RGB10A2_UINT:                    
                case GliFormat.RGB10A2_SINT:                    
                case GliFormat.BGR10A2_UNORM:                    
                case GliFormat.BGR10A2_SNORM:                    
                case GliFormat.BGR10A2_USCALED:                    
                case GliFormat.BGR10A2_SSCALED:                    
                case GliFormat.BGR10A2_UINT:                    
                case GliFormat.BGR10A2_SINT:                    
                case GliFormat.R16_UNORM:                    
                case GliFormat.R16_SNORM:                    
                case GliFormat.R16_USCALED:                    
                case GliFormat.R16_SSCALED:                    
                case GliFormat.R16_UINT:                    
                case GliFormat.R16_SINT:                   
                case GliFormat.R16_SFLOAT:                    
                case GliFormat.RG16_UNORM:                    
                case GliFormat.RG16_SNORM:                    
                case GliFormat.RG16_USCALED:                    
                case GliFormat.RG16_SSCALED:                    
                case GliFormat.RG16_UINT:                    
                case GliFormat.RG16_SINT:                    
                case GliFormat.RG16_SFLOAT:                    
                case GliFormat.RGB16_UNORM:                   
                case GliFormat.RGB16_SNORM:                    
                case GliFormat.RGB16_USCALED:                    
                case GliFormat.RGB16_SSCALED:                    
                case GliFormat.RGB16_UINT:                    
                case GliFormat.RGB16_SINT:                   
                case GliFormat.RGB16_SFLOAT:                    
                case GliFormat.RGBA16_UNORM:                    
                case GliFormat.RGBA16_SNORM:                    
                case GliFormat.RGBA16_USCALED:                    
                case GliFormat.RGBA16_SSCALED:                    
                case GliFormat.RGBA16_UINT:                    
                case GliFormat.RGBA16_SINT:                    
                case GliFormat.RGBA16_SFLOAT:                    
                case GliFormat.R32_UINT:                    
                case GliFormat.R32_SINT:                    
                case GliFormat.R32_SFLOAT:                    
                case GliFormat.RG32_UINT:                    
                case GliFormat.RG32_SINT:                  
                case GliFormat.RG32_SFLOAT:                   
                case GliFormat.RGB32_UINT:                    
                case GliFormat.RGB32_SINT:                    
                case GliFormat.RGB32_SFLOAT:                    
                case GliFormat.RGBA32_UINT:                    
                case GliFormat.RGBA32_SINT:                    
                case GliFormat.RGBA32_SFLOAT:                    
                case GliFormat.R64_UINT:
                case GliFormat.R64_SINT:                    
                case GliFormat.R64_SFLOAT:                    
                case GliFormat.RG64_UINT:                    
                case GliFormat.RG64_SINT:                    
                case GliFormat.RG64_SFLOAT:                    
                case GliFormat.RGB64_UINT:                    
                case GliFormat.RGB64_SINT:                   
                case GliFormat.RGB64_SFLOAT:                    
                case GliFormat.RGBA64_UINT:                    
                case GliFormat.RGBA64_SINT:                   
                case GliFormat.RGBA64_SFLOAT:                 
                case GliFormat.RG11B10_UFLOAT:                    
                case GliFormat.RGB9E5_UFLOAT:                    
                case GliFormat.D16_UNORM:
                case GliFormat.D24_UNORM_PACK32:
                case GliFormat.D32_SFLOAT:
                case GliFormat.S8_UINT:
                case GliFormat.D16_UNORM_S8_UINT_PACK32:
                case GliFormat.D24_UNORM_S8_UINT:
                case GliFormat.D32_SFLOAT_S8_UINT_PACK64:
                case GliFormat.L16_UNORM:
                case GliFormat.A16_UNORM:
                case GliFormat.LA16_UNORM:
                    return false;

                // compressed formats are all 8 bit i think
                case GliFormat.RGB_DXT1_UNORM_BLOCK8:                    
                case GliFormat.RGB_DXT1_SRGB_BLOCK8:                    
                case GliFormat.RGBA_DXT1_UNORM_BLOCK8:                    
                case GliFormat.RGBA_DXT1_SRGB_BLOCK8:                   
                case GliFormat.RGBA_DXT3_UNORM_BLOCK16:                    
                case GliFormat.RGBA_DXT3_SRGB_BLOCK16:                    
                case GliFormat.RGBA_DXT5_UNORM_BLOCK16:                    
                case GliFormat.RGBA_DXT5_SRGB_BLOCK16:                    
                case GliFormat.R_ATI1N_UNORM_BLOCK8:
                case GliFormat.R_ATI1N_SNORM_BLOCK8:
                case GliFormat.RG_ATI2N_UNORM_BLOCK16:
                case GliFormat.RG_ATI2N_SNORM_BLOCK16:
                case GliFormat.RGB_BP_UFLOAT_BLOCK16:
                case GliFormat.RGB_BP_SFLOAT_BLOCK16:
                case GliFormat.RGBA_BP_UNORM_BLOCK16:
                case GliFormat.RGBA_BP_SRGB_BLOCK16:
                case GliFormat.RGB_ETC2_UNORM_BLOCK8:
                case GliFormat.RGB_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK8:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK16:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK16:
                case GliFormat.R_EAC_UNORM_BLOCK8:
                case GliFormat.R_EAC_SNORM_BLOCK8:
                case GliFormat.RG_EAC_UNORM_BLOCK16:
                case GliFormat.RG_EAC_SNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_4X4_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_4X4_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_5X4_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_5X4_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_5X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_5X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_6X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_6X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_6X6_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_6X6_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_8X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_8X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_8X6_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_8X6_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_8X8_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_8X8_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X6_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X6_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X8_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X8_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X10_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X10_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_12X10_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_12X10_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_12X12_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_12X12_SRGB_BLOCK16:
                case GliFormat.RGB_PVRTC1_8X8_UNORM_BLOCK32:
                case GliFormat.RGB_PVRTC1_8X8_SRGB_BLOCK32:
                case GliFormat.RGB_PVRTC1_16X8_UNORM_BLOCK32:
                case GliFormat.RGB_PVRTC1_16X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC1_8X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC1_8X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC1_16X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC1_16X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC2_4X4_UNORM_BLOCK8:
                case GliFormat.RGBA_PVRTC2_4X4_SRGB_BLOCK8:
                case GliFormat.RGBA_PVRTC2_8X4_UNORM_BLOCK8:
                case GliFormat.RGBA_PVRTC2_8X4_SRGB_BLOCK8:
                case GliFormat.RGB_ETC_UNORM_BLOCK8:
                case GliFormat.RGB_ATC_UNORM_BLOCK8:
                case GliFormat.RGBA_ATCA_UNORM_BLOCK16:
                case GliFormat.RGBA_ATCI_UNORM_BLOCK16:
                    return true;
            }

            return false;
        }

        private ExportModel.LdrMode GetLdrMode(GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RGBA4_UNORM:
                case GliFormat.BGRA4_UNORM:
                case GliFormat.R5G6B5_UNORM:
                case GliFormat.B5G6R5_UNORM:
                case GliFormat.RGB5A1_UNORM:
                case GliFormat.BGR5A1_UNORM:
                case GliFormat.A1RGB5_UNORM:
                case GliFormat.R8_UNORM:
                case GliFormat.RG8_UNORM:
                case GliFormat.RGB8_UNORM:
                case GliFormat.BGR8_UNORM:
                case GliFormat.RGBA8_UNORM:
                case GliFormat.BGRA8_UNORM:
                case GliFormat.RGBA8_UNORM_PACK32:
                case GliFormat.L8_UNORM:
                case GliFormat.A8_UNORM:
                case GliFormat.LA8_UNORM:
                case GliFormat.BGR8_UNORM_PACK32:
                case GliFormat.RG3B2_UNORM:
                case GliFormat.RGB10A2_UNORM:
                case GliFormat.BGR10A2_UNORM:
                case GliFormat.R16_UNORM:
                case GliFormat.RG16_UNORM:
                case GliFormat.RGB16_UNORM:
                case GliFormat.RGBA16_UNORM:
                case GliFormat.D16_UNORM:
                case GliFormat.D24_UNORM_PACK32:
                case GliFormat.D16_UNORM_S8_UINT_PACK32:
                case GliFormat.D24_UNORM_S8_UINT:
                case GliFormat.L16_UNORM:
                case GliFormat.A16_UNORM:
                case GliFormat.LA16_UNORM:
                case GliFormat.RGB_DXT1_UNORM_BLOCK8:
                case GliFormat.RGBA_DXT1_UNORM_BLOCK8:
                case GliFormat.RGBA_DXT3_UNORM_BLOCK16:
                case GliFormat.RGBA_DXT5_UNORM_BLOCK16:
                case GliFormat.R_ATI1N_UNORM_BLOCK8:
                case GliFormat.RG_ATI2N_UNORM_BLOCK16:
                case GliFormat.RGBA_BP_UNORM_BLOCK16:
                case GliFormat.RGB_ETC2_UNORM_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK16:
                case GliFormat.R_EAC_UNORM_BLOCK8:
                case GliFormat.RG_EAC_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_4X4_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_5X4_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_5X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_6X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_6X6_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_8X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_8X6_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_8X8_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X5_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X6_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X8_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_10X10_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_12X10_UNORM_BLOCK16:
                case GliFormat.RGBA_ASTC_12X12_UNORM_BLOCK16:
                case GliFormat.RGB_PVRTC1_8X8_UNORM_BLOCK32:
                case GliFormat.RGB_PVRTC1_16X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC1_8X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC1_16X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC2_4X4_UNORM_BLOCK8:
                case GliFormat.RGBA_PVRTC2_8X4_UNORM_BLOCK8:
                case GliFormat.RGB_ETC_UNORM_BLOCK8:
                case GliFormat.RGB_ATC_UNORM_BLOCK8:
                case GliFormat.RGBA_ATCA_UNORM_BLOCK16:
                case GliFormat.RGBA_ATCI_UNORM_BLOCK16:
                    return ExportModel.LdrMode.UNorm;

                case GliFormat.R8_SNORM:
                case GliFormat.RG8_SNORM:
                case GliFormat.RGB8_SNORM:
                case GliFormat.BGR8_SNORM:
                case GliFormat.RGBA8_SNORM:
                case GliFormat.BGRA8_SNORM:
                case GliFormat.RGBA8_SNORM_PACK32:
                case GliFormat.RGB10A2_SNORM:
                case GliFormat.BGR10A2_SNORM:
                case GliFormat.R16_SNORM:
                case GliFormat.RG16_SNORM:
                case GliFormat.RGB16_SNORM:
                case GliFormat.RGBA16_SNORM:
                case GliFormat.R_ATI1N_SNORM_BLOCK8:
                case GliFormat.RG_ATI2N_SNORM_BLOCK16:
                case GliFormat.R_EAC_SNORM_BLOCK8:
                case GliFormat.RG_EAC_SNORM_BLOCK16:
                    return ExportModel.LdrMode.SNorm;

                case GliFormat.R8_SRGB:
                case GliFormat.RG8_SRGB:
                case GliFormat.RGB8_SRGB:
                case GliFormat.BGR8_SRGB:
                case GliFormat.RGBA8_SRGB:
                case GliFormat.BGRA8_SRGB:
                case GliFormat.RGBA8_SRGB_PACK32:
                case GliFormat.BGR8_SRGB_PACK32:
                case GliFormat.RGB_DXT1_SRGB_BLOCK8:
                case GliFormat.RGBA_DXT1_SRGB_BLOCK8:
                case GliFormat.RGBA_DXT3_SRGB_BLOCK16:
                case GliFormat.RGBA_DXT5_SRGB_BLOCK16:
                case GliFormat.RGBA_BP_SRGB_BLOCK16:
                case GliFormat.RGB_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_4X4_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_5X4_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_5X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_6X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_6X6_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_8X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_8X6_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_8X8_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X5_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X6_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X8_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_10X10_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_12X10_SRGB_BLOCK16:
                case GliFormat.RGBA_ASTC_12X12_SRGB_BLOCK16:
                case GliFormat.RGB_PVRTC1_8X8_SRGB_BLOCK32:
                case GliFormat.RGB_PVRTC1_16X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC1_8X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC1_16X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC2_4X4_SRGB_BLOCK8:
                case GliFormat.RGBA_PVRTC2_8X4_SRGB_BLOCK8:
                    return ExportModel.LdrMode.Srgb;

                case GliFormat.R8_USCALED:
                case GliFormat.R8_SSCALED:
                case GliFormat.R8_UINT:
                case GliFormat.R8_SINT:
                case GliFormat.RG8_USCALED:
                case GliFormat.RG8_SSCALED:
                case GliFormat.RG8_UINT:
                case GliFormat.RG8_SINT:
                case GliFormat.RGB8_USCALED:
                case GliFormat.RGB8_SSCALED:
                case GliFormat.RGB8_UINT:
                case GliFormat.RGB8_SINT:
                case GliFormat.BGR8_USCALED:
                case GliFormat.BGR8_SSCALED:
                case GliFormat.BGR8_UINT:
                case GliFormat.BGR8_SINT:
                case GliFormat.RGBA8_USCALED:
                case GliFormat.RGBA8_SSCALED:
                case GliFormat.RGBA8_UINT:
                case GliFormat.RGBA8_SINT:
                case GliFormat.BGRA8_USCALED:
                case GliFormat.BGRA8_SSCALED:
                case GliFormat.BGRA8_UINT:
                case GliFormat.BGRA8_SINT:
                case GliFormat.RGBA8_USCALED_PACK32:
                case GliFormat.RGBA8_SSCALED_PACK32:
                case GliFormat.RGBA8_UINT_PACK32:
                case GliFormat.RGBA8_SINT_PACK32:
                case GliFormat.RGB10A2_USCALED:
                case GliFormat.RGB10A2_SSCALED:
                case GliFormat.RGB10A2_UINT:
                case GliFormat.RGB10A2_SINT:
                case GliFormat.BGR10A2_USCALED:
                case GliFormat.BGR10A2_SSCALED:
                case GliFormat.BGR10A2_UINT:
                case GliFormat.BGR10A2_SINT:
                case GliFormat.R16_USCALED:
                case GliFormat.R16_SSCALED:
                case GliFormat.R16_UINT:
                case GliFormat.R16_SINT:
                case GliFormat.R16_SFLOAT:
                case GliFormat.RG16_USCALED:
                case GliFormat.RG16_SSCALED:
                case GliFormat.RG16_UINT:
                case GliFormat.RG16_SINT:
                case GliFormat.RG16_SFLOAT:
                case GliFormat.RGB16_USCALED:
                case GliFormat.RGB16_SSCALED:
                case GliFormat.RGB16_UINT:
                case GliFormat.RGB16_SINT:
                case GliFormat.RGB16_SFLOAT:
                case GliFormat.RGBA16_USCALED:
                case GliFormat.RGBA16_SSCALED:
                case GliFormat.RGBA16_UINT:
                case GliFormat.RGBA16_SINT:
                case GliFormat.RGBA16_SFLOAT:
                case GliFormat.R32_UINT:
                case GliFormat.R32_SINT:
                case GliFormat.R32_SFLOAT:
                case GliFormat.RG32_UINT:
                case GliFormat.RG32_SINT:
                case GliFormat.RG32_SFLOAT:
                case GliFormat.RGB32_UINT:
                case GliFormat.RGB32_SINT:
                case GliFormat.RGB32_SFLOAT:
                case GliFormat.RGBA32_UINT:
                case GliFormat.RGBA32_SINT:
                case GliFormat.RGBA32_SFLOAT:
                case GliFormat.R64_UINT:
                case GliFormat.R64_SINT:
                case GliFormat.R64_SFLOAT:
                case GliFormat.RG64_UINT:
                case GliFormat.RG64_SINT:
                case GliFormat.RG64_SFLOAT:
                case GliFormat.RGB64_UINT:
                case GliFormat.RGB64_SINT:
                case GliFormat.RGB64_SFLOAT:
                case GliFormat.RGBA64_UINT:
                case GliFormat.RGBA64_SINT:
                case GliFormat.RGBA64_SFLOAT:
                case GliFormat.RG11B10_UFLOAT:
                case GliFormat.RGB9E5_UFLOAT:
                case GliFormat.D32_SFLOAT:
                case GliFormat.S8_UINT:
                case GliFormat.D32_SFLOAT_S8_UINT_PACK64:
                case GliFormat.RGB_BP_UFLOAT_BLOCK16:
                case GliFormat.RGB_BP_SFLOAT_BLOCK16:
                    return ExportModel.LdrMode.Undefined;
            }

            return ExportModel.LdrMode.Undefined;
        }
    }
}
