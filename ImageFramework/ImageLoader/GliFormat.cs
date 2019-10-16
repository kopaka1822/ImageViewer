using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public enum GliFormat
    {
        UNDEFINED = 0,

        FORMAT_FIRST, RG4_UNORM = FORMAT_FIRST,
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

        RGB_DXT1_UNORM_BLOCK8,
        RGB_DXT1_SRGB_BLOCK8,
        RGBA_DXT1_UNORM_BLOCK8,
        RGBA_DXT1_SRGB_BLOCK8,
        RGBA_DXT3_UNORM_BLOCK16,
        RGBA_DXT3_SRGB_BLOCK16,
        RGBA_DXT5_UNORM_BLOCK16,
        RGBA_DXT5_SRGB_BLOCK16,
        R_ATI1N_UNORM_BLOCK8,
        R_ATI1N_SNORM_BLOCK8,
        RG_ATI2N_UNORM_BLOCK16,
        RG_ATI2N_SNORM_BLOCK16,
        RGB_BP_UFLOAT_BLOCK16,
        RGB_BP_SFLOAT_BLOCK16,
        RGBA_BP_UNORM_BLOCK16,
        RGBA_BP_SRGB_BLOCK16,

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

        RG3B2_UNORM, LAST = RG3B2_UNORM
    };

    public static class GliFormatExtensions
    {
        public static bool IsCompressed(this GliFormat format)
        {
            switch (format)
            {
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

        /// <summary>
        /// uncompressed formats that have less than 8 bit precision
        /// </summary>
        public static bool IsLessThan8Bit(this GliFormat format)
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
                case GliFormat.RG3B2_UNORM:
                    return true;
            }

            return false;
        }
    }
}
