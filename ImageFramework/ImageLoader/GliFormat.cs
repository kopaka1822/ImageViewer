using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public enum GliFormat
    {
        UNDEFINED = 0,

        RG4_UNORM,
        RGBA4_UNORM,
        BGRA4_UNORM,
        R5G6B5_UNORM,
        B5G6R5_UNORM,
        RGB5A1_UNORM,
        BGR5A1_UNORM,
        A1RGB5_UNORM,

        R8_UNORM,
        R8_SNORM,
        R8_USCALED,
        R8_SSCALED,
        R8_UINT,
        R8_SINT,
        R8_SRGB,

        RG8_UNORM,
        RG8_SNORM,
        RG8_USCALED,
        RG8_SSCALED,
        RG8_UINT,
        RG8_SINT,
        RG8_SRGB,

        RGB8_UNORM,
        RGB8_SNORM,
        RGB8_USCALED,
        RGB8_SSCALED,
        RGB8_UINT,
        RGB8_SINT,
        RGB8_SRGB,

        BGR8_UNORM,
        BGR8_SNORM,
        BGR8_USCALED,
        BGR8_SSCALED,
        BGR8_UINT,
        BGR8_SINT,
        BGR8_SRGB,

        RGBA8_UNORM,
        RGBA8_SNORM,
        RGBA8_USCALED,
        RGBA8_SSCALED,
        RGBA8_UINT,
        RGBA8_SINT,
        RGBA8_SRGB,

        BGRA8_UNORM,
        BGRA8_SNORM,
        BGRA8_USCALED,
        BGRA8_SSCALED,
        BGRA8_UINT,
        BGRA8_SINT,
        BGRA8_SRGB,

        RGBA8_UNORM_PACK32,
        RGBA8_SNORM_PACK32,
        RGBA8_USCALED_PACK32,
        RGBA8_SSCALED_PACK32,
        RGBA8_UINT_PACK32,
        RGBA8_SINT_PACK32,
        RGBA8_SRGB_PACK32,

        RGB10A2_UNORM,
        RGB10A2_SNORM,
        RGB10A2_USCALED,
        RGB10A2_SSCALED,
        RGB10A2_UINT,
        RGB10A2_SINT,

        BGR10A2_UNORM,
        BGR10A2_SNORM,
        BGR10A2_USCALED,
        BGR10A2_SSCALED,
        BGR10A2_UINT,
        BGR10A2_SINT,

        R16_UNORM,
        R16_SNORM,
        R16_USCALED,
        R16_SSCALED,
        R16_UINT,
        R16_SINT,
        R16_SFLOAT,

        RG16_UNORM,
        RG16_SNORM,
        RG16_USCALED,
        RG16_SSCALED,
        RG16_UINT,
        RG16_SINT,
        RG16_SFLOAT,

        RGB16_UNORM,
        RGB16_SNORM,
        RGB16_USCALED,
        RGB16_SSCALED,
        RGB16_UINT,
        RGB16_SINT,
        RGB16_SFLOAT,

        RGBA16_UNORM,
        RGBA16_SNORM,
        RGBA16_USCALED,
        RGBA16_SSCALED,
        RGBA16_UINT,
        RGBA16_SINT,
        RGBA16_SFLOAT,

        R32_UINT,
        R32_SINT,
        R32_SFLOAT,

        RG32_UINT,
        RG32_SINT,
        RG32_SFLOAT,

        RGB32_UINT,
        RGB32_SINT,
        RGB32_SFLOAT,

        RGBA32_UINT,
        RGBA32_SINT,
        RGBA32_SFLOAT,

        R64_UINT,
        R64_SINT,
        R64_SFLOAT,

        RG64_UINT,
        RG64_SINT,
        RG64_SFLOAT,

        RGB64_UINT,
        RGB64_SINT,
        RGB64_SFLOAT,

        RGBA64_UINT,
        RGBA64_SINT,
        RGBA64_SFLOAT,

        RG11B10_UFLOAT,
        RGB9E5_UFLOAT,

        D16_UNORM,
        D24_UNORM_PACK32,
        D32_SFLOAT,
        S8_UINT,
        D16_UNORM_S8_UINT_PACK32,
        D24_UNORM_S8_UINT,
        D32_SFLOAT_S8_UINT_PACK64,

        RGB_DXT1_UNORM,
        RGB_DXT1_SRGB,
        RGBA_DXT1_UNORM,
        RGBA_DXT1_SRGB,
        RGBA_DXT3_UNORM,
        RGBA_DXT3_SRGB,
        RGBA_DXT5_UNORM,
        RGBA_DXT5_SRGB,
        R_ATI1N_UNORM,
        R_ATI1N_SNORM,
        RG_ATI2N_UNORM,
        RG_ATI2N_SNORM,
        RGB_BP_UFLOAT,
        RGB_BP_SFLOAT,
        RGBA_BP_UNORM,
        RGBA_BP_SRGB,

        RGB_ETC2_UNORM_BLOCK8,
        RGB_ETC2_SRGB_BLOCK8,
        RGBA_ETC2_UNORM_BLOCK8,
        RGBA_ETC2_SRGB_BLOCK8,
        RGBA_ETC2_UNORM_BLOCK16,
        RGBA_ETC2_SRGB_BLOCK16,
        R_EAC_UNORM_BLOCK8,
        R_EAC_SNORM_BLOCK8,
        RG_EAC_UNORM_BLOCK16,
        RG_EAC_SNORM_BLOCK16,

        RGBA_ASTC_4X4_UNORM_BLOCK16,
        RGBA_ASTC_4X4_SRGB_BLOCK16,
        RGBA_ASTC_5X4_UNORM_BLOCK16,
        RGBA_ASTC_5X4_SRGB_BLOCK16,
        RGBA_ASTC_5X5_UNORM_BLOCK16,
        RGBA_ASTC_5X5_SRGB_BLOCK16,
        RGBA_ASTC_6X5_UNORM_BLOCK16,
        RGBA_ASTC_6X5_SRGB_BLOCK16,
        RGBA_ASTC_6X6_UNORM_BLOCK16,
        RGBA_ASTC_6X6_SRGB_BLOCK16,
        RGBA_ASTC_8X5_UNORM_BLOCK16,
        RGBA_ASTC_8X5_SRGB_BLOCK16,
        RGBA_ASTC_8X6_UNORM_BLOCK16,
        RGBA_ASTC_8X6_SRGB_BLOCK16,
        RGBA_ASTC_8X8_UNORM_BLOCK16,
        RGBA_ASTC_8X8_SRGB_BLOCK16,
        RGBA_ASTC_10X5_UNORM_BLOCK16,
        RGBA_ASTC_10X5_SRGB_BLOCK16,
        RGBA_ASTC_10X6_UNORM_BLOCK16,
        RGBA_ASTC_10X6_SRGB_BLOCK16,
        RGBA_ASTC_10X8_UNORM_BLOCK16,
        RGBA_ASTC_10X8_SRGB_BLOCK16,
        RGBA_ASTC_10X10_UNORM_BLOCK16,
        RGBA_ASTC_10X10_SRGB_BLOCK16,
        RGBA_ASTC_12X10_UNORM_BLOCK16,
        RGBA_ASTC_12X10_SRGB_BLOCK16,
        RGBA_ASTC_12X12_UNORM_BLOCK16,
        RGBA_ASTC_12X12_SRGB_BLOCK16,

        RGB_PVRTC1_8X8_UNORM_BLOCK32,
        RGB_PVRTC1_8X8_SRGB_BLOCK32,
        RGB_PVRTC1_16X8_UNORM_BLOCK32,
        RGB_PVRTC1_16X8_SRGB_BLOCK32,
        RGBA_PVRTC1_8X8_UNORM_BLOCK32,
        RGBA_PVRTC1_8X8_SRGB_BLOCK32,
        RGBA_PVRTC1_16X8_UNORM_BLOCK32,
        RGBA_PVRTC1_16X8_SRGB_BLOCK32,
        RGBA_PVRTC2_4X4_UNORM_BLOCK8,
        RGBA_PVRTC2_4X4_SRGB_BLOCK8,
        RGBA_PVRTC2_8X4_UNORM_BLOCK8,
        RGBA_PVRTC2_8X4_SRGB_BLOCK8,

        RGB_ETC_UNORM_BLOCK8,
        RGB_ATC_UNORM_BLOCK8,
        RGBA_ATCA_UNORM_BLOCK16,
        RGBA_ATCI_UNORM_BLOCK16,

        L8_UNORM,
        A8_UNORM,
        LA8_UNORM,
        L16_UNORM,
        A16_UNORM,
        LA16_UNORM,

        BGR8_UNORM_PACK32,
        BGR8_SRGB_PACK32,

        RG3B2_UNORM, LAST = RG3B2_UNORM,

        // extensions from libpng
        RA8_SRGB,
        RA8_UNORM,
        AR8_SRGB,
        ARGB8_SRGB,
        ABGR8_SRGB,
        RA16_UNORM
    };

    public enum PixelDataType
    {
        Undefined,
        UNorm,
        SNorm,
        Srgb,
        SInt,
        UInt,
        SScaled,
        UScaled,
        SFloat,
        UFloat
    }

    public static class GliFormatExtensions
    {
        public static bool IsCompressed(this GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RGB_DXT1_UNORM:                    
                case GliFormat.RGB_DXT1_SRGB:                    
                case GliFormat.RGBA_DXT1_UNORM:                    
                case GliFormat.RGBA_DXT1_SRGB:                    
                case GliFormat.RGBA_DXT3_UNORM:                    
                case GliFormat.RGBA_DXT3_SRGB:                    
                case GliFormat.RGBA_DXT5_UNORM:                    
                case GliFormat.RGBA_DXT5_SRGB:                    
                case GliFormat.R_ATI1N_UNORM:                    
                case GliFormat.R_ATI1N_SNORM:                    
                case GliFormat.RG_ATI2N_UNORM:                    
                case GliFormat.RG_ATI2N_SNORM:
                case GliFormat.RGB_BP_UFLOAT:
                case GliFormat.RGB_BP_SFLOAT:
                case GliFormat.RGBA_BP_UNORM:                    
                case GliFormat.RGBA_BP_SRGB:                    
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

        /// <summary>
        /// returns required alignment or 0 is not required
        /// </summary>
        public static int GetAlignmentX(this GliFormat format)
        {
            if (!format.IsCompressed()) return 0;

            // true for all BC formats. The rest is not supported atm.
            return 4;
        }

        /// <summary>
        /// returns required alignment or 0 is not required
        /// </summary>
        public static int GetAlignmentY(this GliFormat format)
        {
            if (!format.IsCompressed()) return 0;

            // true for all BC formats. The rest is not supported atm.
            return 4;
        }

        public static bool IsAtMost8bit(this GliFormat format)
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
                case GliFormat.RA8_SRGB:
                case GliFormat.RA8_UNORM:
                case GliFormat.AR8_SRGB:
                case GliFormat.ARGB8_SRGB:
                case GliFormat.ABGR8_SRGB:
                    return true;

                /*case GliFormat.RGB10A2_UNORM:
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

                // hdr compressed
                case GliFormat.RGB_BP_UFLOAT_BLOCK16:
                case GliFormat.RGB_BP_SFLOAT_BLOCK16:
                    return false;*/

                // compressed formats are all 8 bit i think
                case GliFormat.RGB_DXT1_UNORM:
                case GliFormat.RGB_DXT1_SRGB:
                case GliFormat.RGBA_DXT1_UNORM:
                case GliFormat.RGBA_DXT1_SRGB:
                case GliFormat.RGBA_DXT3_UNORM:
                case GliFormat.RGBA_DXT3_SRGB:
                case GliFormat.RGBA_DXT5_UNORM:
                case GliFormat.RGBA_DXT5_SRGB:
                case GliFormat.R_ATI1N_UNORM:
                case GliFormat.R_ATI1N_SNORM:
                case GliFormat.RG_ATI2N_UNORM:
                case GliFormat.RG_ATI2N_SNORM:
                case GliFormat.RGBA_BP_UNORM:
                case GliFormat.RGBA_BP_SRGB:
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

        /// <summary>
        /// uncompressed formats that have less than 8 bit precision
        /// </summary>
        public static bool IsLessThan8Bit(this GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RG4_UNORM:
                case GliFormat.RGBA4_UNORM:
                case GliFormat.BGRA4_UNORM:
                case GliFormat.R5G6B5_UNORM:
                case GliFormat.B5G6R5_UNORM:
                case GliFormat.RGB5A1_UNORM:
                case GliFormat.BGR5A1_UNORM:
                case GliFormat.A1RGB5_UNORM:
                case GliFormat.RG3B2_UNORM:
                    return true;
            }

            return false;
        }

        public static bool Is8Bit(this GliFormat format)
        {
            return !IsLessThan8Bit(format) && IsAtMost8bit(format);
        }

        // some formats don't work for texture3D targets
        public static bool IsExcludedFrom3DExport(this GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RG16_UNORM:
                case GliFormat.R_ATI1N_SNORM:
                case GliFormat.RG_ATI2N_SNORM:
                case GliFormat.RGB_BP_UFLOAT:
                    return true;
            }

            return false;
        }

        public static bool HasAlpha(this GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RGBA4_UNORM:
                case GliFormat.BGRA4_UNORM:
                case GliFormat.RGB5A1_UNORM:
                case GliFormat.BGR5A1_UNORM:
                case GliFormat.A1RGB5_UNORM:
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
                case GliFormat.RGBA16_UNORM:
                case GliFormat.RGBA16_SNORM:
                case GliFormat.RGBA16_USCALED:
                case GliFormat.RGBA16_SSCALED:
                case GliFormat.RGBA16_UINT:
                case GliFormat.RGBA16_SINT:
                case GliFormat.RGBA16_SFLOAT:
                case GliFormat.RGBA32_UINT:
                case GliFormat.RGBA32_SINT:
                case GliFormat.RGBA32_SFLOAT:
                case GliFormat.RGBA64_UINT:
                case GliFormat.RGBA64_SINT:
                case GliFormat.RGBA64_SFLOAT:
                case GliFormat.RGBA_DXT1_UNORM:
                case GliFormat.RGBA_DXT1_SRGB:
                case GliFormat.RGBA_DXT3_UNORM:
                case GliFormat.RGBA_DXT3_SRGB:
                case GliFormat.RGBA_DXT5_UNORM:
                case GliFormat.RGBA_DXT5_SRGB:
                case GliFormat.RGBA_BP_UNORM:
                case GliFormat.RGBA_BP_SRGB:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK8:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK16:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK16:
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
                case GliFormat.RGBA_PVRTC1_8X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC1_8X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC1_16X8_UNORM_BLOCK32:
                case GliFormat.RGBA_PVRTC1_16X8_SRGB_BLOCK32:
                case GliFormat.RGBA_PVRTC2_4X4_UNORM_BLOCK8:
                case GliFormat.RGBA_PVRTC2_4X4_SRGB_BLOCK8:
                case GliFormat.RGBA_PVRTC2_8X4_UNORM_BLOCK8:
                case GliFormat.RGBA_PVRTC2_8X4_SRGB_BLOCK8:
                case GliFormat.RGBA_ATCA_UNORM_BLOCK16:
                case GliFormat.RGBA_ATCI_UNORM_BLOCK16:
                case GliFormat.A8_UNORM:
                case GliFormat.LA8_UNORM:
                case GliFormat.A16_UNORM:
                case GliFormat.LA16_UNORM:
                case GliFormat.RA8_SRGB:
                case GliFormat.RA8_UNORM:
                case GliFormat.AR8_SRGB:
                case GliFormat.ABGR8_SRGB:
                case GliFormat.ARGB8_SRGB:
                case GliFormat.RA16_UNORM:
                    return true;
            }

            return false;
        }

        public static bool HasRgb(this GliFormat format)
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
                case GliFormat.RGB32_UINT:
                case GliFormat.RGB32_SINT:
                case GliFormat.RGB32_SFLOAT:
                case GliFormat.RGBA32_UINT:
                case GliFormat.RGBA32_SINT:
                case GliFormat.RGBA32_SFLOAT:
                case GliFormat.RGB64_UINT:
                case GliFormat.RGB64_SINT:
                case GliFormat.RGB64_SFLOAT:
                case GliFormat.RGBA64_UINT:
                case GliFormat.RGBA64_SINT:
                case GliFormat.RGBA64_SFLOAT:
                case GliFormat.RG11B10_UFLOAT:
                case GliFormat.RGB9E5_UFLOAT:
                case GliFormat.RGB_DXT1_UNORM:
                case GliFormat.RGB_DXT1_SRGB:
                case GliFormat.RGBA_DXT1_UNORM:
                case GliFormat.RGBA_DXT1_SRGB:
                case GliFormat.RGBA_DXT3_UNORM:
                case GliFormat.RGBA_DXT3_SRGB:
                case GliFormat.RGBA_DXT5_UNORM:
                case GliFormat.RGBA_DXT5_SRGB:
                case GliFormat.RGB_BP_UFLOAT:
                case GliFormat.RGB_BP_SFLOAT:
                case GliFormat.RGBA_BP_UNORM:
                case GliFormat.RGBA_BP_SRGB:
                case GliFormat.RGB_ETC2_UNORM_BLOCK8:
                case GliFormat.RGB_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK8:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK16:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK16:
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
                case GliFormat.BGR8_UNORM_PACK32:
                case GliFormat.BGR8_SRGB_PACK32:
                case GliFormat.RG3B2_UNORM:
                case GliFormat.ARGB8_SRGB:
                case GliFormat.ABGR8_SRGB:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// helpful texture description (mostly for compressed formats)
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetDescription(this GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RGB9E5_UFLOAT:
                    return "Three partial-precision floating-point numbers encoded into a single 32-bit value all sharing the same 5-bit exponent. There is no sign bit, and there is a shared 5-bit biased (15) exponent and a 9-bit mantissa for each channel";

                case GliFormat.RGB_DXT1_UNORM:
                case GliFormat.RGB_DXT1_SRGB:
                    return "Three color channels (5 bits:6 bits:5 bits) (BC1)";

                case GliFormat.RGBA_DXT1_UNORM:
                case GliFormat.RGBA_DXT1_SRGB:
                    return "Three color channels (5 bits:6 bits:5 bits), with 1 bit of alpha (BC1)";

                case GliFormat.RGBA_DXT3_UNORM:
                case GliFormat.RGBA_DXT3_SRGB:
                    return "Three color channels (5 bits:6 bits:5 bits), with 4 bits of alpha (BC2)";

                case GliFormat.RGBA_DXT5_UNORM:
                case GliFormat.RGBA_DXT5_SRGB:
                    return "Three color channels (5 bits:6 bits:5 bits) with 8 bits of alpha (BC3)";

                case GliFormat.R_ATI1N_UNORM:
                case GliFormat.R_ATI1N_SNORM:
                    return "One color channel (8 bits) (BC4)";

                case GliFormat.RG_ATI2N_UNORM:
                case GliFormat.RG_ATI2N_SNORM:
                    return "Two color channels (8 bits:8 bits) (GreenBlue) (BC5)";

                case GliFormat.RGB_BP_UFLOAT:
                case GliFormat.RGB_BP_SFLOAT:
                    return "Three color channels (16 bits:16 bits:16 bits) in \"half\" floating point (BC6). \"Half\" floating point is a 16 bit value that consists of an optional sign bit, a 5 bit biased exponent, and a 10 or 11 bit mantissa";

                case GliFormat.RGBA_BP_UNORM:
                case GliFormat.RGBA_BP_SRGB:
                    return "Three color channels (4 to 7 bits per channel) with 0 to 8 bits of alpha (BC7)";

                /*case GliFormat.RGB_ETC2_UNORM_BLOCK8:
                case GliFormat.RGB_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK8:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK8:
                case GliFormat.RGBA_ETC2_UNORM_BLOCK16:
                case GliFormat.RGBA_ETC2_SRGB_BLOCK16:
                case GliFormat.R_EAC_UNORM_BLOCK8:
                case GliFormat.R_EAC_SNORM_BLOCK8:
                case GliFormat.RG_EAC_UNORM_BLOCK16:
                case GliFormat.RG_EAC_SNORM_BLOCK16:*/
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
                    return "Adaptive Scalable Texture Compression (lossy block-based)";
                /*case GliFormat.RGB_PVRTC1_8X8_UNORM_BLOCK32:
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
                    break;*/
            }

            return "";
        }

        public static PixelDataType GetDataType(this GliFormat format)
        {
            switch (format)
            {
                case GliFormat.RG4_UNORM:
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
                case GliFormat.RGB_DXT1_UNORM:
                case GliFormat.RGBA_DXT1_UNORM:
                case GliFormat.RGBA_DXT3_UNORM:
                case GliFormat.RGBA_DXT5_UNORM:
                case GliFormat.R_ATI1N_UNORM:
                case GliFormat.RG_ATI2N_UNORM:
                case GliFormat.RGBA_BP_UNORM:
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
                case GliFormat.RA8_UNORM:
                case GliFormat.RA16_UNORM:
                    return PixelDataType.UNorm;

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
                case GliFormat.R_ATI1N_SNORM:
                case GliFormat.RG_ATI2N_SNORM:
                case GliFormat.R_EAC_SNORM_BLOCK8:
                case GliFormat.RG_EAC_SNORM_BLOCK16:
                    return PixelDataType.SNorm;

                case GliFormat.R8_SRGB:
                case GliFormat.RG8_SRGB:
                case GliFormat.RGB8_SRGB:
                case GliFormat.BGR8_SRGB:
                case GliFormat.RGBA8_SRGB:
                case GliFormat.BGRA8_SRGB:
                case GliFormat.RGBA8_SRGB_PACK32:
                case GliFormat.BGR8_SRGB_PACK32:
                case GliFormat.RGB_DXT1_SRGB:
                case GliFormat.RGBA_DXT1_SRGB:
                case GliFormat.RGBA_DXT3_SRGB:
                case GliFormat.RGBA_DXT5_SRGB:
                case GliFormat.RGBA_BP_SRGB:
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
                case GliFormat.RA8_SRGB:
                case GliFormat.AR8_SRGB:
                case GliFormat.ARGB8_SRGB:
                case GliFormat.ABGR8_SRGB:
                    return PixelDataType.Srgb;

                case GliFormat.R8_USCALED:
                case GliFormat.RG8_USCALED:
                case GliFormat.RGB8_USCALED:
                case GliFormat.BGR8_USCALED:
                case GliFormat.RGBA8_USCALED:
                case GliFormat.BGRA8_USCALED:
                case GliFormat.RGBA8_USCALED_PACK32:
                case GliFormat.RGB10A2_USCALED:
                case GliFormat.BGR10A2_USCALED:
                case GliFormat.R16_USCALED:
                case GliFormat.RG16_USCALED:
                case GliFormat.RGB16_USCALED:
                case GliFormat.RGBA16_USCALED:
                    return PixelDataType.UScaled;

                case GliFormat.R8_SSCALED:
                case GliFormat.RG8_SSCALED:
                case GliFormat.RGB8_SSCALED:
                case GliFormat.BGR8_SSCALED:
                case GliFormat.RGBA8_SSCALED:
                case GliFormat.BGRA8_SSCALED:
                case GliFormat.RGBA8_SSCALED_PACK32:
                case GliFormat.RGB10A2_SSCALED:
                case GliFormat.BGR10A2_SSCALED:
                case GliFormat.R16_SSCALED:
                case GliFormat.RG16_SSCALED:
                case GliFormat.RGB16_SSCALED:
                case GliFormat.RGBA16_SSCALED:
                    return PixelDataType.SScaled;

                case GliFormat.R8_UINT:
                case GliFormat.RG8_UINT:
                case GliFormat.RGB8_UINT:
                case GliFormat.BGR8_UINT:
                case GliFormat.RGBA8_UINT:
                case GliFormat.BGRA8_UINT:
                case GliFormat.RGBA8_UINT_PACK32:
                case GliFormat.RGB10A2_UINT:
                case GliFormat.BGR10A2_UINT:
                case GliFormat.R16_UINT:
                case GliFormat.RG16_UINT:
                case GliFormat.RGB16_UINT:
                case GliFormat.RGBA16_UINT:
                case GliFormat.R32_UINT:
                case GliFormat.RG32_UINT:
                case GliFormat.RGB32_UINT:
                case GliFormat.RGBA32_UINT:
                case GliFormat.R64_UINT:
                case GliFormat.RG64_UINT:
                case GliFormat.RGB64_UINT:
                case GliFormat.RGBA64_UINT:
                case GliFormat.S8_UINT:
                    return PixelDataType.UInt;

                case GliFormat.R8_SINT:
                case GliFormat.RG8_SINT:
                case GliFormat.RGB8_SINT:
                case GliFormat.BGR8_SINT:
                case GliFormat.RGBA8_SINT:
                case GliFormat.BGRA8_SINT:
                case GliFormat.RGBA8_SINT_PACK32:
                case GliFormat.RGB10A2_SINT:
                case GliFormat.BGR10A2_SINT:
                case GliFormat.RG16_SINT:
                case GliFormat.RGB16_SINT:
                case GliFormat.RGBA16_SINT:
                case GliFormat.R32_SINT:
                case GliFormat.RG32_SINT:
                case GliFormat.RGB32_SINT:
                case GliFormat.RGBA32_SINT:
                case GliFormat.R64_SINT:
                case GliFormat.RG64_SINT:
                case GliFormat.RGB64_SINT:
                case GliFormat.RGBA64_SINT:
                case GliFormat.R16_SINT:
                    return PixelDataType.SInt;

                case GliFormat.R16_SFLOAT:
                case GliFormat.RG16_SFLOAT:
                case GliFormat.RGB16_SFLOAT:
                case GliFormat.RGBA16_SFLOAT:
                case GliFormat.R32_SFLOAT:
                case GliFormat.RG32_SFLOAT:
                case GliFormat.RGB32_SFLOAT:
                case GliFormat.RGBA32_SFLOAT:
                case GliFormat.R64_SFLOAT:
                case GliFormat.RG64_SFLOAT:
                case GliFormat.RGB64_SFLOAT:
                case GliFormat.RGBA64_SFLOAT:
                case GliFormat.D32_SFLOAT:
                case GliFormat.RGB_BP_SFLOAT:
                case GliFormat.D32_SFLOAT_S8_UINT_PACK64:
                    return PixelDataType.SFloat;

                case GliFormat.RG11B10_UFLOAT:
                case GliFormat.RGB9E5_UFLOAT:
                case GliFormat.RGB_BP_UFLOAT:
                    return PixelDataType.UFloat;
                default:
                    return PixelDataType.Undefined;
            }
        }

        public static bool IsSigned(this PixelDataType type)
        {
            switch (type)
            {
                case PixelDataType.SNorm:
                case PixelDataType.SInt:
                case PixelDataType.SScaled:
                case PixelDataType.SFloat:
                    return true;
            }

            return false;
        }

        // indicates if the formats values are between 0 and 1
        public static bool IsUnormed(this PixelDataType type)
        {
            switch (type)
            {
                case PixelDataType.UNorm:
                case PixelDataType.Srgb:
                    return true;
            }

            return false;
        }

        public static string GetDescription(this PixelDataType type)
        {
            switch (type)
            {
                case PixelDataType.UNorm:
                    return "Unsigned integer interpreted as evenly spaced floating point with range [0, 1]";
                case PixelDataType.SNorm:
                    return "Signed integer interpreted as evenly spaced floating point with range [-1, 1]";
                case PixelDataType.Srgb:
                    return
                        "Unsigned integer interpreted as nonlinear progressing floating points with range [0, 1]. Usually used for 8bit colors";
                case PixelDataType.SInt:
                    return "Signed integer";
                case PixelDataType.UInt:
                    return "Unsigned integer";
                case PixelDataType.SScaled:
                    return "Signed integer interpreted as floating point";
                case PixelDataType.UScaled:
                    return "Unsigned integer interpreted as floating point";
                case PixelDataType.SFloat:
                    return "Signed floating point";
                case PixelDataType.UFloat:
                    return "Unsigned floating point";
            }

            return "";
        }
    }
}
