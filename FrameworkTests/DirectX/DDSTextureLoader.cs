// DDSTextureLoader Ported to C# by Justin Stenning, March 2017
//--------------------------------------------------------------------------------------
// File: DDSTextureLoader.cpp
//
// Functions for loading a DDS texture and creating a Direct3D runtime resource for it
//
// Note these functions are useful as a light-weight runtime loader for DDS files. For
// a full-featured DDS file reader, writer, and texture processing pipeline see
// the 'Texconv' sample and the 'DirectXTex' library.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// http://go.microsoft.com/fwlink/?LinkId=248926
// http://go.microsoft.com/fwlink/?LinkId=248929
//--------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;

namespace FrameworkTests.DirectX
{ 
    public static class DDSTextureLoader
    {
        public enum DDS_ALPHA_MODE
        {
            DDS_ALPHA_MODE_UNKNOWN = 0,
            DDS_ALPHA_MODE_STRAIGHT = 1,
            DDS_ALPHA_MODE_PREMULTIPLIED = 2,
            DDS_ALPHA_MODE_OPAQUE = 3,
            DDS_ALPHA_MODE_CUSTOM = 4,
        };

        const int DDS_MAGIC = 0x20534444;// "DDS "

        [StructLayout(LayoutKind.Sequential)]
        struct DDS_PIXELFORMAT
        {
            public int size;
            public int flags;
            public int fourCC;
            public int RGBBitCount;
            public uint RBitMask;
            public uint GBitMask;
            public uint BBitMask;
            public uint ABitMask;
        };

        const int DDS_FOURCC = 0x00000004;// DDPF_FOURCC
        const int DDS_RGB = 0x00000040;// DDPF_RGB
        const int DDS_RGBA = 0x00000041;// DDPF_RGB | DDPF_ALPHAPIXELS
        const int DDS_LUMINANCE = 0x00020000;// DDPF_LUMINANCE
        const int DDS_LUMINANCEA = 0x00020001;// DDPF_LUMINANCE | DDPF_ALPHAPIXELS
        const int DDS_ALPHA = 0x00000002;// DDPF_ALPHA
        const int DDS_PAL8 = 0x00000020;// DDPF_PALETTEINDEXED8

        const int DDS_HEADER_FLAGS_TEXTURE = 0x00001007;// DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_PIXELFORMAT
        const int DDS_HEADER_FLAGS_MIPMAP = 0x00020000;// DDSD_MIPMAPCOUNT
        const int DDS_HEADER_FLAGS_VOLUME = 0x00800000;// DDSD_DEPTH
        const int DDS_HEADER_FLAGS_PITCH = 0x00000008;// DDSD_PITCH
        const int DDS_HEADER_FLAGS_LINEARSIZE = 0x00080000;// DDSD_LINEARSIZE

        const int DDS_HEIGHT = 0x00000002;// DDSD_HEIGHT
        const int DDS_WIDTH = 0x00000004;// DDSD_WIDTH

        const int DDS_SURFACE_FLAGS_TEXTURE = 0x00001000;// DDSCAPS_TEXTURE
        const int DDS_SURFACE_FLAGS_MIPMAP = 0x00400008;// DDSCAPS_COMPLEX | DDSCAPS_MIPMAP
        const int DDS_SURFACE_FLAGS_CUBEMAP = 0x00000008;// DDSCAPS_COMPLEX

        const int DDS_CUBEMAP_POSITIVEX = 0x00000600;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEX
        const int DDS_CUBEMAP_NEGATIVEX = 0x00000a00;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEX
        const int DDS_CUBEMAP_POSITIVEY = 0x00001200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEY
        const int DDS_CUBEMAP_NEGATIVEY = 0x00002200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEY
        const int DDS_CUBEMAP_POSITIVEZ = 0x00004200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_POSITIVEZ
        const int DDS_CUBEMAP_NEGATIVEZ = 0x00008200;// DDSCAPS2_CUBEMAP | DDSCAPS2_CUBEMAP_NEGATIVEZ

        const int DDS_CUBEMAP_ALLFACES = (DDS_CUBEMAP_POSITIVEX | DDS_CUBEMAP_NEGATIVEX | DDS_CUBEMAP_POSITIVEY | DDS_CUBEMAP_NEGATIVEY | DDS_CUBEMAP_POSITIVEZ | DDS_CUBEMAP_NEGATIVEZ);

        const int DDS_CUBEMAP = 0x00000200;// DDSCAPS2_CUBEMAP

        const int DDS_FLAGS_VOLUME = 0x00200000;// DDSCAPS2_VOLUME

        [StructLayout(LayoutKind.Sequential)]
        struct DDS_HEADER
        {
            public int size;
            public int flags;
            public int height;
            public int width;
            public int pitchOrLinearSize;
            public int depth; // only if DDS_HEADER_FLAGS_VOLUME is set in flags
            public int mipMapCount;
            //===11
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public int[] reserved1;

            public DDS_PIXELFORMAT ddspf;
            public int caps;
            public int caps2;
            public int caps3;
            public int caps4;
            public int reserved2;


        }

        enum DDS_MISC_FLAGS2
        {
            DDS_MISC_FLAGS2_ALPHA_MODE_MASK = 0x7,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct DDS_HEADER_DXT10
        {
            public Format dxgiFormat;
            public ResourceDimension resourceDimension;
            public ResourceOptionFlags miscFlag; // see D3D11_RESOURCE_MISC_FLAG
            public int arraySize;
            public int miscFlags2;
        }


        static int BitsPerPixel(Format fmt)
        {
            switch (fmt)
            {
                case Format.R32G32B32A32_Typeless:
                case Format.R32G32B32A32_Float:
                case Format.R32G32B32A32_UInt:
                case Format.R32G32B32A32_SInt:
                    return 128;

                case Format.R32G32B32_Typeless:
                case Format.R32G32B32_Float:
                case Format.R32G32B32_UInt:
                case Format.R32G32B32_SInt:
                    return 96;

                case Format.R16G16B16A16_Typeless:
                case Format.R16G16B16A16_Float:
                case Format.R16G16B16A16_UNorm:
                case Format.R16G16B16A16_UInt:
                case Format.R16G16B16A16_SNorm:
                case Format.R16G16B16A16_SInt:
                case Format.R32G32_Typeless:
                case Format.R32G32_Float:
                case Format.R32G32_UInt:
                case Format.R32G32_SInt:
                case Format.R32G8X24_Typeless:
                case Format.D32_Float_S8X24_UInt:
                case Format.R32_Float_X8X24_Typeless:
                case Format.X32_Typeless_G8X24_UInt:
                    return 64;

                case Format.R10G10B10A2_Typeless:
                case Format.R10G10B10A2_UNorm:
                case Format.R10G10B10A2_UInt:
                case Format.R11G11B10_Float:
                case Format.R8G8B8A8_Typeless:
                case Format.R8G8B8A8_UNorm:
                case Format.R8G8B8A8_UNorm_SRgb:
                case Format.R8G8B8A8_UInt:
                case Format.R8G8B8A8_SNorm:
                case Format.R8G8B8A8_SInt:
                case Format.R16G16_Typeless:
                case Format.R16G16_Float:
                case Format.R16G16_UNorm:
                case Format.R16G16_UInt:
                case Format.R16G16_SNorm:
                case Format.R16G16_SInt:
                case Format.R32_Typeless:
                case Format.D32_Float:
                case Format.R32_Float:
                case Format.R32_UInt:
                case Format.R32_SInt:
                case Format.R24G8_Typeless:
                case Format.D24_UNorm_S8_UInt:
                case Format.R24_UNorm_X8_Typeless:
                case Format.X24_Typeless_G8_UInt:
                case Format.R9G9B9E5_Sharedexp:
                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                case Format.B8G8R8A8_UNorm:
                case Format.B8G8R8X8_UNorm:
                case Format.R10G10B10_Xr_Bias_A2_UNorm:
                case Format.B8G8R8A8_Typeless:
                case Format.B8G8R8A8_UNorm_SRgb:
                case Format.B8G8R8X8_Typeless:
                case Format.B8G8R8X8_UNorm_SRgb:
                    return 32;

                case Format.R8G8_Typeless:
                case Format.R8G8_UNorm:
                case Format.R8G8_UInt:
                case Format.R8G8_SNorm:
                case Format.R8G8_SInt:
                case Format.R16_Typeless:
                case Format.R16_Float:
                case Format.D16_UNorm:
                case Format.R16_UNorm:
                case Format.R16_UInt:
                case Format.R16_SNorm:
                case Format.R16_SInt:
                case Format.B5G6R5_UNorm:
                case Format.B5G5R5A1_UNorm:
                case Format.B4G4R4A4_UNorm:
                    return 16;

                case Format.R8_Typeless:
                case Format.R8_UNorm:
                case Format.R8_UInt:
                case Format.R8_SNorm:
                case Format.R8_SInt:
                case Format.A8_UNorm:
                    return 8;

                case Format.R1_UNorm:
                    return 1;

                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    return 4;

                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.BC6H_Typeless:
                case Format.BC6H_Uf16:
                case Format.BC6H_Sf16:
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    return 8;

                default:
                    return 0;
            }
        }

        static DDS_ALPHA_MODE GetAlphaMode(DDS_HEADER header, IntPtr headerPtr)
        {
            if ((header.ddspf.flags & DDS_FOURCC) > 0)
            {
                if (MAKEFOURCC('D', 'X', '1', '0') == header.ddspf.fourCC)
                {
                    var d3d10ext = (DDS_HEADER_DXT10)Marshal.PtrToStructure(headerPtr + Marshal.SizeOf(typeof(DDS_HEADER)), typeof(DDS_HEADER_DXT10));
                    var mode = (DDS_ALPHA_MODE)(d3d10ext.miscFlags2 & (int)DDS_MISC_FLAGS2.DDS_MISC_FLAGS2_ALPHA_MODE_MASK);
                    switch (mode)
                    {
                        case DDS_ALPHA_MODE.DDS_ALPHA_MODE_STRAIGHT:
                        case DDS_ALPHA_MODE.DDS_ALPHA_MODE_PREMULTIPLIED:
                        case DDS_ALPHA_MODE.DDS_ALPHA_MODE_OPAQUE:
                        case DDS_ALPHA_MODE.DDS_ALPHA_MODE_CUSTOM:
                            return mode;
                    }
                }
                else if ((MAKEFOURCC('D', 'X', 'T', '2') == header.ddspf.fourCC)
                    || (MAKEFOURCC('D', 'X', 'T', '4') == header.ddspf.fourCC))
                {
                    return DDS_ALPHA_MODE.DDS_ALPHA_MODE_PREMULTIPLIED;
                }
            }

            return DDS_ALPHA_MODE.DDS_ALPHA_MODE_UNKNOWN;
        }

        //--------------------------------------------------------------------------------------
        // Get surface information for a particular format
        //--------------------------------------------------------------------------------------
        static void GetSurfaceInfo(int width, int height, Format fmt, out int outNumBytes, out int outRowBytes, out int outNumRows)
        {
            int numBytes = 0;
            int rowBytes = 0;
            int numRows = 0;

            bool bc = false;
            bool packed = false;
            int bcnumBytesPerBlock = 0;
            switch (fmt)
            {
                case Format.BC1_Typeless:
                case Format.BC1_UNorm:
                case Format.BC1_UNorm_SRgb:
                case Format.BC4_Typeless:
                case Format.BC4_UNorm:
                case Format.BC4_SNorm:
                    bc = true;
                    bcnumBytesPerBlock = 8;
                    break;

                case Format.BC2_Typeless:
                case Format.BC2_UNorm:
                case Format.BC2_UNorm_SRgb:
                case Format.BC3_Typeless:
                case Format.BC3_UNorm:
                case Format.BC3_UNorm_SRgb:
                case Format.BC5_Typeless:
                case Format.BC5_UNorm:
                case Format.BC5_SNorm:
                case Format.BC6H_Typeless:
                case Format.BC6H_Uf16:
                case Format.BC6H_Sf16:
                case Format.BC7_Typeless:
                case Format.BC7_UNorm:
                case Format.BC7_UNorm_SRgb:
                    bc = true;
                    bcnumBytesPerBlock = 16;
                    break;

                case Format.R8G8_B8G8_UNorm:
                case Format.G8R8_G8B8_UNorm:
                    packed = true;
                    break;
            }

            if (bc)
            {
                int numBlocksWide = 0;
                if (width > 0)
                {
                    numBlocksWide = Math.Max(1, (width + 3) / 4);
                }
                int numBlocksHigh = 0;
                if (height > 0)
                {
                    numBlocksHigh = Math.Max(1, (height + 3) / 4);
                }
                rowBytes = numBlocksWide * bcnumBytesPerBlock;
                numRows = numBlocksHigh;
            }
            else if (packed)
            {
                rowBytes = ((width + 1) >> 1) * 4;
                numRows = height;
            }
            else
            {
                int bpp = BitsPerPixel(fmt);
                rowBytes = (width * bpp + 7) / 8; // round up to nearest byte
                numRows = height;
            }

            numBytes = rowBytes * numRows;

            outNumBytes = numBytes;
            outRowBytes = rowBytes;
            outNumRows = numRows;
        }


        static bool ISBITMASK(DDS_PIXELFORMAT ddpf, uint r, uint g, uint b, uint a)
        {
            return (ddpf.RBitMask == r && ddpf.GBitMask == g && ddpf.BBitMask == b && ddpf.ABitMask == a);
        }

        static int MAKEFOURCC(int ch0, int ch1, int ch2, int ch3)
        {
            return ((int)(byte)(ch0) | ((int)(byte)(ch1) << 8) | ((int)(byte)(ch2) << 16) | ((int)(byte)(ch3) << 24));
        }


        static Format GetDXGIFormat(DDS_PIXELFORMAT ddpf)
        {

            if ((ddpf.flags & DDS_RGB) > 0)
            {
                // Note that sRGB formats are written using the "DX10" extended header

                switch (ddpf.RGBBitCount)
                {
                    case 32:
                        if (ISBITMASK(ddpf, 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000))
                        {
                            return Format.R8G8B8A8_UNorm;
                        }

                        if (ISBITMASK(ddpf, 0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000))
                        {
                            return Format.B8G8R8A8_UNorm;
                        }

                        if (ISBITMASK(ddpf, 0x00ff0000, 0x0000ff00, 0x000000ff, 0x00000000))
                        {
                            return Format.B8G8R8X8_UNorm;
                        }

                        // No DXGI format maps to ISBITMASK(0x000000ff, 0x0000ff00, 0x00ff0000, 0x00000000) aka D3DFMT_X8B8G8R8

                        // Note that many common DDS reader/writers (including D3DX) swap the
                        // the RED/BLUE masks for 10:10:10:2 formats. We assumme
                        // below that the 'backwards' header mask is being used since it is most
                        // likely written by D3DX. The more robust solution is to use the 'DX10'
                        // header extension and specify the DXGI_FORMAT_R10G10B10A2_UNORM format directly

                        // For 'correct' writers, this should be 0x000003ff, 0x000ffc00, 0x3ff00000 for RGB data
                        if (ISBITMASK(ddpf, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000))
                        {
                            return Format.R10G10B10A2_UNorm;
                        }

                        // No DXGI format maps to ISBITMASK(0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000) aka D3DFMT_A2R10G10B10

                        if (ISBITMASK(ddpf, 0x0000ffff, 0xffff0000, 0x00000000, 0x00000000))
                        {
                            return Format.R16G16_UNorm;
                        }

                        if (ISBITMASK(ddpf, 0xffffffff, 0x00000000, 0x00000000, 0x00000000))
                        {
                            // Only 32-bit color channel format in D3D9 was R32F
                            return Format.R32_Float; // D3DX writes this out as a FourCC of 114
                        }
                        break;

                    case 24:
                        // No 24bpp DXGI formats aka D3DFMT_R8G8B8
                        break;

                    case 16:
                        if (ISBITMASK(ddpf, 0x7c00, 0x03e0, 0x001f, 0x8000))
                        {
                            return Format.B5G5R5A1_UNorm;
                        }
                        if (ISBITMASK(ddpf, 0xf800, 0x07e0, 0x001f, 0x0000))
                        {
                            return Format.B5G6R5_UNorm;
                        }

                        // No DXGI format maps to ISBITMASK(0x7c00, 0x03e0, 0x001f, 0x0000) aka D3DFMT_X1R5G5B5
                        if (ISBITMASK(ddpf, 0x0f00, 0x00f0, 0x000f, 0xf000))
                        {
                            return Format.B4G4R4A4_UNorm;
                        }

                        // No DXGI format maps to ISBITMASK(0x0f00, 0x00f0, 0x000f, 0x0000) aka D3DFMT_X4R4G4B4

                        // No 3:3:2, 3:3:2:8, or paletted DXGI formats aka D3DFMT_A8R3G3B2, D3DFMT_R3G3B2, D3DFMT_P8, D3DFMT_A8P8, etc.
                        break;
                }
            }
            else if ((ddpf.flags & DDS_LUMINANCE) > 0)
            {
                if (8 == ddpf.RGBBitCount)
                {
                    if (ISBITMASK(ddpf, 0x000000ff, 0x00000000, 0x00000000, 0x00000000))
                    {
                        return Format.R8_UNorm; // D3DX10/11 writes this out as DX10 extension
                    }

                    // No DXGI format maps to ISBITMASK(0x0f, 0x00, 0x00, 0xf0) aka D3DFMT_A4L4
                }

                if (16 == ddpf.RGBBitCount)
                {
                    if (ISBITMASK(ddpf, 0x0000ffff, 0x00000000, 0x00000000, 0x00000000))
                    {
                        return Format.R16_UNorm; // D3DX10/11 writes this out as DX10 extension
                    }
                    if (ISBITMASK(ddpf, 0x000000ff, 0x00000000, 0x00000000, 0x0000ff00))
                    {
                        return Format.R8G8_UNorm; // D3DX10/11 writes this out as DX10 extension
                    }
                }
            }
            else if ((ddpf.flags & DDS_ALPHA) > 0)
            {
                if (8 == ddpf.RGBBitCount)
                {
                    return Format.A8_UNorm;
                }
            }
            else if ((ddpf.flags & DDS_FOURCC) > 0)
            {
                if (MAKEFOURCC('D', 'X', 'T', '1') == ddpf.fourCC)
                {
                    return Format.BC1_UNorm;
                }
                if (MAKEFOURCC('D', 'X', 'T', '3') == ddpf.fourCC)
                {
                    return Format.BC2_UNorm;
                }
                if (MAKEFOURCC('D', 'X', 'T', '5') == ddpf.fourCC)
                {
                    return Format.BC3_UNorm;
                }

                // While pre-mulitplied alpha isn't directly supported by the DXGI formats,
                // they are basically the same as these BC formats so they can be mapped
                if (MAKEFOURCC('D', 'X', 'T', '2') == ddpf.fourCC)
                {
                    return Format.BC2_UNorm;
                }
                if (MAKEFOURCC('D', 'X', 'T', '4') == ddpf.fourCC)
                {
                    return Format.BC3_UNorm;
                }

                if (MAKEFOURCC('A', 'T', 'I', '1') == ddpf.fourCC)
                {
                    return Format.BC4_UNorm;
                }
                if (MAKEFOURCC('B', 'C', '4', 'U') == ddpf.fourCC)
                {
                    return Format.BC4_UNorm;
                }
                if (MAKEFOURCC('B', 'C', '4', 'S') == ddpf.fourCC)
                {
                    return Format.BC4_SNorm;
                }

                if (MAKEFOURCC('A', 'T', 'I', '2') == ddpf.fourCC)
                {
                    return Format.BC5_UNorm;
                }
                if (MAKEFOURCC('B', 'C', '5', 'U') == ddpf.fourCC)
                {
                    return Format.BC5_UNorm;
                }
                if (MAKEFOURCC('B', 'C', '5', 'S') == ddpf.fourCC)
                {
                    return Format.BC5_SNorm;
                }

                // BC6H and BC7 are written using the "DX10" extended header

                if (MAKEFOURCC('R', 'G', 'B', 'G') == ddpf.fourCC)
                {
                    return Format.R8G8_B8G8_UNorm;
                }
                if (MAKEFOURCC('G', 'R', 'G', 'B') == ddpf.fourCC)
                {
                    return Format.G8R8_G8B8_UNorm;
                }

                // Check for D3DFORMAT enums being set here
                switch (ddpf.fourCC)
                {
                    case 36: // D3DFMT_A16B16G16R16
                        return Format.R16G16B16A16_UNorm;

                    case 110: // D3DFMT_Q16W16V16U16
                        return Format.R16G16B16A16_SNorm;

                    case 111: // D3DFMT_R16F
                        return Format.R16_Float;

                    case 112: // D3DFMT_G16R16F
                        return Format.R16G16_Float;

                    case 113: // D3DFMT_A16B16G16R16F
                        return Format.R16G16B16A16_Float;

                    case 114: // D3DFMT_R32F
                        return Format.R32_Float;

                    case 115: // D3DFMT_G32R32F
                        return Format.R32G32_Float;

                    case 116: // D3DFMT_A32B32G32R32F
                        return Format.R32G32B32A32_Float;
                }
            }

            return Format.Unknown;
        }

        //--------------------------------------------------------------------------------------
        static bool FillInitData(int width,
            int height,
            int depth,
            int mipCount,
            int arraySize,
            Format format,
            int maxsize,
            int bitSize,
            IntPtr bitData,
            out int twidth,
            out int theight,
            out int tdepth,
            out int skipMip,
            DataBox[] initData)
        {
            if (bitData == null)
                throw new ArgumentNullException("bitData");
            if (initData == null)
                throw new ArgumentNullException("initData");

            skipMip = 0;
            twidth = 0;
            theight = 0;
            tdepth = 0;

            int NumBytes = 0;
            int RowBytes = 0;
            IntPtr pSrcBits = bitData;
            IntPtr pEndBits = bitData + bitSize;

            int index = 0;
            for (int j = 0; j < arraySize; j++)
            {
                int w = width;
                int h = height;
                int d = depth;
                for (int i = 0; i < mipCount; i++)
                {
                    int NumRows;
                    GetSurfaceInfo(w,
                        h,
                        format,
                        out NumBytes,
                        out RowBytes,
                        out NumRows
                    );

                    if ((mipCount <= 1) || maxsize == 0 || (w <= maxsize && h <= maxsize && d <= maxsize))
                    {
                        if (twidth == 0)
                        {
                            twidth = w;
                            theight = h;
                            tdepth = d;
                        }

                        System.Diagnostics.Debug.Assert(index < mipCount * arraySize);
                        initData[index].DataPointer = pSrcBits;
                        initData[index].RowPitch = RowBytes;
                        initData[index].SlicePitch = NumBytes;
                        ++index;
                    }
                    else if (j == 0)
                    {
                        // Count number of skipped mipmaps (first item only)
                        ++skipMip;
                    }

                    if ((pSrcBits + (NumBytes * d)).ToInt64() > pEndBits.ToInt64())
                    {
                        throw new System.IO.EndOfStreamException();
                    }

                    pSrcBits += NumBytes * d;

                    w = w >> 1;
                    h = h >> 1;
                    d = d >> 1;
                    if (w == 0)
                    {
                        w = 1;
                    }
                    if (h == 0)
                    {
                        h = 1;
                    }
                    if (d == 0)
                    {
                        d = 1;
                    }
                }
            }

            return (index > 0);
        }

        //--------------------------------------------------------------------------------------
        static Format MakeSRGB(Format format)
        {
            switch (format)
            {
                case Format.R8G8B8A8_UNorm:
                    return Format.R8G8B8A8_UNorm_SRgb;

                case Format.BC1_UNorm:
                    return Format.BC1_UNorm_SRgb;

                case Format.BC2_UNorm:
                    return Format.BC2_UNorm_SRgb;

                case Format.BC3_UNorm:
                    return Format.BC3_UNorm_SRgb;

                case Format.B8G8R8A8_UNorm:
                    return Format.B8G8R8A8_UNorm_SRgb;

                case Format.B8G8R8X8_UNorm:
                    return Format.B8G8R8X8_UNorm_SRgb;

                case Format.BC7_UNorm:
                    return Format.BC7_UNorm_SRgb;

                default:
                    return format;
            }
        }

        //--------------------------------------------------------------------------------------
        static SharpDX.Result CreateD3DResources(SharpDX.Direct3D11.Device d3dDevice,
            ResourceDimension resDim,
            int width,
            int height,
            int depth,
            int mipCount,
            int arraySize,
            Format format,

            ResourceUsage usage,
            BindFlags bindFlags,
            CpuAccessFlags CpuAccessFlags,
            ResourceOptionFlags miscFlags,
            bool forceSRGB,
            bool isCubeMap,
            DataBox[] initData,
             out Resource texture,
             out ShaderResourceView textureView)
        {
            if (d3dDevice == null)
                throw new ArgumentNullException("d3dDevice");

            SharpDX.Result result = Result.Fail;
            texture = null;
            textureView = null;

            if (forceSRGB)
            {
                format = MakeSRGB(format);
            }

            ShaderResourceViewDescription SRVDesc = new ShaderResourceViewDescription();
            SRVDesc.Format = format;

            switch (resDim)
            {
                case ResourceDimension.Texture1D:
                    {
                        Texture1DDescription desc;
                        desc.Width = width;
                        desc.MipLevels = mipCount;
                        desc.ArraySize = arraySize;
                        desc.Format = format;
                        desc.Usage = usage;
                        desc.BindFlags = bindFlags;
                        desc.CpuAccessFlags = CpuAccessFlags;
                        desc.OptionFlags = miscFlags & ~ResourceOptionFlags.TextureCube;

                        Texture1D tex = null;
                        tex = new Texture1D(d3dDevice, desc, initData);

                        if (tex != null)
                        {
                            if (arraySize > 1)
                            {
                                SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1DArray;
                                SRVDesc.Texture1DArray.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;
                                SRVDesc.Texture1DArray.ArraySize = arraySize;
                            }
                            else
                            {
                                SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1D;
                                SRVDesc.Texture1D.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;
                            }
                            textureView = new ShaderResourceView(d3dDevice, tex, SRVDesc);

                            if (textureView == null)
                            {
                                tex.Dispose();
                                texture = null;
                                return result;
                            }


                            texture = tex;
                        }
                    }
                    break;

                case ResourceDimension.Texture2D:
                    {
                        Texture2DDescription desc = new Texture2DDescription();

                        desc.Width = width;
                        desc.Height = height;
                        desc.MipLevels = mipCount;
                        desc.ArraySize = arraySize;
                        desc.Format = format;
                        desc.SampleDescription = new SampleDescription();
                        desc.SampleDescription.Count = 1;
                        desc.SampleDescription.Quality = 0;
                        desc.Usage = usage;
                        desc.BindFlags = bindFlags;
                        desc.CpuAccessFlags = CpuAccessFlags;
                        if (isCubeMap)
                        {
                            desc.OptionFlags = miscFlags | ResourceOptionFlags.TextureCube;
                        }
                        else
                        {
                            desc.OptionFlags = miscFlags & ResourceOptionFlags.TextureCube;
                        }

                        Texture2D tex = new Texture2D(d3dDevice, desc, initData);
                        if (tex != null)
                        {


                            if (isCubeMap)
                            {
                                if (arraySize > 6)
                                {
                                    SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
                                    SRVDesc.TextureCubeArray.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;

                                    // Earlier we set arraySize to (NumCubes * 6)
                                    SRVDesc.TextureCubeArray.CubeCount = arraySize / 6;
                                }
                                else
                                {
                                    SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
                                    SRVDesc.TextureCube.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;
                                }
                            }
                            else if (arraySize > 1)
                            {
                                SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;
                                SRVDesc.Texture2DArray.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;
                                SRVDesc.Texture2DArray.ArraySize = arraySize;
                            }
                            else
                            {
                                SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                                SRVDesc.Texture2D.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;
                            }

                            textureView = new ShaderResourceView(d3dDevice, tex, SRVDesc);
                            if (textureView == null)
                            {
                                tex.Dispose();
                                tex = null;
                                texture = null;
                                return result;
                            }


                            texture = tex;
                        }
                    }
                    break;

                case ResourceDimension.Texture3D:
                    {
                        Texture3DDescription desc = new Texture3DDescription();

                        desc.Width = width;
                        desc.Height = height;
                        desc.Depth = depth;
                        desc.MipLevels = mipCount;
                        desc.Format = format;
                        desc.Usage = usage;
                        desc.BindFlags = bindFlags;
                        desc.CpuAccessFlags = CpuAccessFlags;
                        desc.OptionFlags = miscFlags & ~ResourceOptionFlags.TextureCube;
                        Texture3D tex = new Texture3D(d3dDevice, desc, initData);

                        if (tex != null)
                        {


                            SRVDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
                            SRVDesc.Texture3D.MipLevels = (mipCount == 0) ? -1 : desc.MipLevels;
                            textureView = new ShaderResourceView(d3dDevice, tex, SRVDesc);
                            if (textureView == null)
                            {
                                tex.Dispose();
                                texture = null;
                                return result;
                            }
                        }

                        texture = tex;
                    }
                    break;
            }

            return Result.Ok;
        }

        //--------------------------------------------------------------------------------------
        static Result CreateTextureFromDDS(SharpDX.Direct3D11.Device d3dDevice,
            DeviceContext d3dContext,
            DDS_HEADER header,
            DDS_HEADER_DXT10? header10,
            IntPtr bitData,
            int bitSize,
            int maxsize,
            ResourceUsage usage,
            BindFlags bindFlags,
            CpuAccessFlags CpuAccessFlags,
            ResourceOptionFlags miscFlags,
            bool forceSRGB,
            out Resource texture,
            out ShaderResourceView textureView)
        {
            Result hr = Result.Ok;
            texture = null;
            textureView = null;

            int width = header.width;
            int height = header.height;
            int depth = header.depth;

            ResourceDimension resDim = ResourceDimension.Unknown;
            int arraySize = 1;
            Format format = Format.Unknown;
            bool isCubeMap = false;

            int mipCount = header.mipMapCount;
            if (0 == mipCount)
            {
                mipCount = 1;
            }

            if ((header.ddspf.flags & DDS_FOURCC) > 0 &&
                (MAKEFOURCC('D', 'X', '1', '0') == header.ddspf.fourCC))
            {
                var d3d10ext = header10.Value;

                arraySize = d3d10ext.arraySize;
                if (arraySize == 0)
                {
                    return Result.Fail;
                }

                switch (d3d10ext.dxgiFormat)
                {
                    case Format.AI44:
                    case Format.IA44:
                    case Format.P8:
                    case Format.A8P8:
                        throw new NotSupportedException(String.Format("{0} DXGI format is not supported", d3d10ext.dxgiFormat.ToString()));
                    default:
                        if (BitsPerPixel(d3d10ext.dxgiFormat) == 0)
                        {
                            throw new NotSupportedException(String.Format("{0} DXGI format is not supported", d3d10ext.dxgiFormat.ToString()));
                        }
                        break;
                }

                format = d3d10ext.dxgiFormat;

                switch (d3d10ext.resourceDimension)
                {
                    case ResourceDimension.Texture1D:
                        // D3DX writes 1D textures with a fixed Height of 1
                        if ((header.flags & DDS_HEIGHT) > 0 && height != 1)
                        {
                            throw new NotSupportedException();
                        }
                        height = depth = 1;
                        break;

                    case ResourceDimension.Texture2D:
                        if ((d3d10ext.miscFlag & ResourceOptionFlags.TextureCube) > 0)
                        {
                            arraySize *= 6;
                            isCubeMap = true;
                        }
                        depth = 1;
                        break;

                    case ResourceDimension.Texture3D:
                        if ((header.flags & DDS_HEADER_FLAGS_VOLUME) == 0)
                        {
                            throw new ArgumentException();
                        }

                        if (arraySize > 1)
                        {
                            throw new ArgumentException();
                        }
                        break;

                    default:
                        throw new ArgumentException();
                }

                resDim = d3d10ext.resourceDimension;
            }
            else
            {
                format = GetDXGIFormat(header.ddspf);

                if (format == Format.Unknown)
                {
                    throw new ArgumentException();
                }

                if ((header.flags & DDS_HEADER_FLAGS_VOLUME) > 0)
                {
                    resDim = ResourceDimension.Texture3D;
                }
                else
                {
                    if ((header.caps2 & DDS_CUBEMAP) > 0)
                    {
                        // We require all six faces to be defined
                        if ((header.caps2 & DDS_CUBEMAP_ALLFACES) != DDS_CUBEMAP_ALLFACES)
                        {
                            throw new ArgumentException();
                        }

                        arraySize = 6;
                        isCubeMap = true;
                    }

                    depth = 1;
                    resDim = ResourceDimension.Texture2D;

                    // Note there's no way for a legacy Direct3D 9 DDS to express a '1D' texture
                }

                System.Diagnostics.Debug.Assert(BitsPerPixel(format) != 0);
            }

            // Bound sizes (for security purposes we don't trust DDS file metadata larger than the Direct3D hardware requirements)
            if (mipCount > Resource.MaximumMipLevels)
            {
                throw new ArgumentException();
            }

            switch (resDim)
            {
                case ResourceDimension.Texture1D:// D3D11_RESOURCE_DIMENSION_TEXTURE1D:
                    if ((arraySize > Resource.MaximumTexture1DArraySize) ||
                        (width > Resource.MaximumTexture1DSize))
                    {
                        throw new ArgumentException();
                    }
                    break;

                case ResourceDimension.Texture2D:
                    if (isCubeMap)
                    {
                        // This is the right bound because we set arraySize to (NumCubes*6) above
                        if ((arraySize > Resource.MaximumTexture2DArraySize) ||
                            (width > Resource.MaximumTextureCubeSize) ||
                            (height > Resource.MaximumTextureCubeSize))
                        {
                            throw new ArgumentException();
                        }
                    }
                    else if ((arraySize > Resource.MaximumTexture2DArraySize) ||
                        (width > Resource.MaximumTexture2DSize) ||
                        (height > Resource.MaximumTexture2DSize))
                    {
                        throw new ArgumentException();
                    }
                    break;

                case ResourceDimension.Texture3D:
                    if ((arraySize > 1) ||
                        (width > Resource.MaximumTexture3DSize) ||
                        (height > Resource.MaximumTexture3DSize) ||
                        (depth > Resource.MaximumTexture3DSize))
                    {
                        throw new ArgumentException();
                    }
                    break;

                default:
                    throw new ArgumentException();
            }

            bool autogen = false;
            if (mipCount == 1) // Must have context and shader-view to auto generate mipmaps
            {
                // See if format is supported for auto-gen mipmaps (varies by feature level)

                var fmtSupport = d3dDevice.CheckFormatSupport(format);

                if ((fmtSupport & FormatSupport.MipAutogen) > 0)
                {
                    // 10level9 feature levels do not support auto-gen mipgen for volume textures
                    if ((resDim != ResourceDimension.Texture3D)
                        || (d3dDevice.FeatureLevel >= SharpDX.Direct3D.FeatureLevel.Level_10_0))
                    {
                        autogen = true;
                    }
                }
            }

            if (autogen)
            {
                // Create texture with auto-generated mipmaps
                Resource tex;

                hr = CreateD3DResources(d3dDevice, resDim, width, height, depth, 0, arraySize,
                    format, usage,
                    bindFlags | BindFlags.RenderTarget,
                    CpuAccessFlags,
                    miscFlags | ResourceOptionFlags.GenerateMipMaps, forceSRGB,
                    isCubeMap, null, out tex, out textureView);
                if (hr == Result.Ok)
                {
                    int numBytes = 0;
                    int rowBytes = 0;
                    int numRows = 0;
                    GetSurfaceInfo(width, height, format, out numBytes, out rowBytes, out numRows);

                    if (numBytes > bitSize)
                    {
                        textureView.Dispose();
                        textureView = null;
                        tex.Dispose();

                        throw new System.IO.EndOfStreamException();
                    }

                    ShaderResourceViewDescription desc = textureView.Description;

                    int mipLevels = 1;

                    switch (desc.Dimension)
                    {
                        case ShaderResourceViewDimension.Texture1D: mipLevels = desc.Texture1D.MipLevels; break;
                        case ShaderResourceViewDimension.Texture1DArray: mipLevels = desc.Texture1DArray.MipLevels; break;
                        case ShaderResourceViewDimension.Texture2D: mipLevels = desc.Texture2D.MipLevels; break;
                        case ShaderResourceViewDimension.Texture2DArray: mipLevels = desc.Texture2DArray.MipLevels; break;
                        case ShaderResourceViewDimension.TextureCube: mipLevels = desc.TextureCube.MipLevels; break;
                        case ShaderResourceViewDimension.TextureCubeArray: mipLevels = desc.TextureCubeArray.MipLevels; break;
                        case ShaderResourceViewDimension.Texture3D: mipLevels = desc.Texture3D.MipLevels; break;
                        default:
                            textureView.Dispose();
                            textureView = null;
                            tex.Dispose();
                            throw new Exception();
                    }

                    if (arraySize > 1)
                    {
                        IntPtr pSrcBits = bitData;
                        IntPtr pEndBits = bitData + bitSize;
                        for (int item = 0; item < arraySize; ++item)
                        {
                            if ((pSrcBits + numBytes).ToInt64() > pEndBits.ToInt64())
                            {
                                textureView.Dispose();
                                textureView = null;
                                tex.Dispose();
                                throw new System.IO.EndOfStreamException();
                            }
                            int res = Resource.CalculateSubResourceIndex(0, item, mipLevels);
                            d3dContext.UpdateSubresource(tex, res, null, pSrcBits, rowBytes, numBytes);
                            pSrcBits += numBytes;
                        }
                    }
                    else
                    {
                        d3dContext.UpdateSubresource(tex, 0, null, bitData, rowBytes, numBytes);
                    }

                    d3dContext.GenerateMips(textureView);
                    texture = tex;
                }
            }
            else
            {
                // Create the texture
                DataBox[] initData = new DataBox[mipCount * arraySize];


                int skipMip = 0;
                int twidth = 0;
                int theight = 0;
                int tdepth = 0;
                if (FillInitData(width, height, depth, mipCount, arraySize, format, maxsize, bitSize, bitData,
                         out twidth, out theight, out tdepth, out skipMip, initData))
                {
                    hr = CreateD3DResources(d3dDevice, resDim, twidth, theight, tdepth, mipCount - skipMip, arraySize,
                        format, usage, bindFlags, CpuAccessFlags, miscFlags, forceSRGB,
                        isCubeMap, initData, out texture, out textureView);

                    if (!hr.Success && maxsize == 0 && (mipCount > 1))
                    {
                        // Retry with a maxsize determined by feature level

                        switch (d3dDevice.FeatureLevel)
                        {
                            case FeatureLevel.Level_9_1:
                            case FeatureLevel.Level_9_2:
                                if (isCubeMap)
                                {
                                    maxsize = 512 /*D3D_FL9_1_REQ_TEXTURECUBE_DIMENSION*/;
                                }
                                else
                                {
                                    maxsize = (resDim == ResourceDimension.Texture3D)
                                        ? 256 /*D3D_FL9_1_REQ_TEXTURE3D_U_V_OR_W_DIMENSION*/
                                        : 2048 /*D3D_FL9_1_REQ_TEXTURE2D_U_OR_V_DIMENSION*/;
                                }
                                break;

                            case FeatureLevel.Level_9_3:
                                maxsize = (resDim == ResourceDimension.Texture3D)
                                    ? 256 /*D3D_FL9_1_REQ_TEXTURE3D_U_V_OR_W_DIMENSION*/
                                    : 4096 /*D3D_FL9_3_REQ_TEXTURE2D_U_OR_V_DIMENSION*/;
                                break;

                            default: // D3D_FEATURE_LEVEL_10_0 & D3D_FEATURE_LEVEL_10_1
                                maxsize = (resDim == ResourceDimension.Texture3D)
                                    ? 2048 /*D3D10_REQ_TEXTURE3D_U_V_OR_W_DIMENSION*/
                                    : 8192 /*D3D10_REQ_TEXTURE2D_U_OR_V_DIMENSION*/;
                                break;
                        }

                        if (FillInitData(width, height, depth, mipCount, arraySize, format, maxsize, bitSize, bitData,
                            out twidth, out theight, out tdepth, out skipMip, initData))
                        {
                            hr = CreateD3DResources(d3dDevice, resDim, twidth, theight, tdepth, mipCount - skipMip, arraySize,
                                format, usage, bindFlags, CpuAccessFlags, miscFlags, forceSRGB,
                                isCubeMap, initData, out texture, out textureView);
                        }
                    }
                }
            }

            return hr;
        }


        //--------------------------------------------------------------------------------------
        public static void CreateDDSTextureFromMemory(Device d3dDevice,
            IntPtr ddsData,
            int ddsDataSize,
            out Resource texture,
            out ShaderResourceView textureView,
            int maxsize,
            out DDS_ALPHA_MODE alphaMode)
        {
            CreateDDSTextureFromMemoryEx(d3dDevice, ddsData, ddsDataSize, maxsize,
                ResourceUsage.Default, BindFlags.ShaderResource, 0, 0, false,
                out texture, out textureView, out alphaMode);
        }

        public static void CreateDDSTextureFromMemory(Device d3dDevice,
            DeviceContext d3dContext,
            IntPtr ddsData,
            int ddsDataSize,
            out Resource texture,
            out ShaderResourceView textureView,
            int maxsize,
            out DDS_ALPHA_MODE alphaMode)
        {
            CreateDDSTextureFromMemoryEx(d3dDevice, d3dContext, ddsData, ddsDataSize, maxsize,
                ResourceUsage.Default, BindFlags.ShaderResource, 0, 0, false,
                out texture, out textureView, out alphaMode);
        }


        public static void CreateDDSTextureFromMemoryEx(Device d3dDevice,
            IntPtr ddsData,
            int ddsDataSize,
            int maxsize,
            ResourceUsage usage,
            BindFlags bindFlags,
            CpuAccessFlags CpuAccessFlags,
            ResourceOptionFlags miscFlags,
            bool forceSRGB,
            out Resource texture,
            out ShaderResourceView textureView,
            out DDS_ALPHA_MODE alphaMode)
        {
            texture = null;
            textureView = null;
            alphaMode = DDS_ALPHA_MODE.DDS_ALPHA_MODE_UNKNOWN;

            var sizeofDDS_HEADER = Marshal.SizeOf(typeof(DDS_HEADER));
            var sizeofDDS_MAGIC = sizeof(int);
            var sizeofDDS_PIXELFORMAT = Marshal.SizeOf(typeof(DDS_PIXELFORMAT));
            var sizeofDDS_HEADER_DXT10 = Marshal.SizeOf(typeof(DDS_HEADER_DXT10));

            if (d3dDevice == null)
                throw new ArgumentNullException("d3dDevice");
            if (ddsData == IntPtr.Zero)
                throw new ArgumentOutOfRangeException("ddsData");

            // Validate DDS file in memory
            if (ddsDataSize < (sizeof(int) + Marshal.SizeOf(typeof(DDS_HEADER))))
            {
                throw new ArgumentOutOfRangeException("ddsDataSize");
            }

            int dwMagicNumber = Marshal.ReadInt32(ddsData);
            if (dwMagicNumber != DDS_MAGIC)
            {
                throw new ArgumentException("Not a valid DDS", "ddsData");
            }

            var header = (DDS_HEADER)Marshal.PtrToStructure(ddsData + sizeofDDS_MAGIC, typeof(DDS_HEADER));
            // Verify header to validate DDS file
            if (header.size != sizeofDDS_HEADER ||
                header.ddspf.size != sizeofDDS_PIXELFORMAT)
            {
                throw new Exception("Invalid DDS");
            }

            // Check for DX10 extension
            bool bDXT10Header = false;
            if ((header.ddspf.flags & DDS_FOURCC) > 0 &&
                (MAKEFOURCC('D', 'X', '1', '0') == header.ddspf.fourCC))
            {
                // Must be long enough for both headers and magic value
                if (ddsDataSize < sizeofDDS_HEADER + sizeofDDS_MAGIC + sizeofDDS_HEADER_DXT10)
                {
                    throw new ArgumentOutOfRangeException("ddsDataSize");
                }

                bDXT10Header = true;
            }

            int offset = sizeofDDS_MAGIC
                + sizeofDDS_HEADER
                + (bDXT10Header ? sizeofDDS_HEADER_DXT10 : 0);
            DDS_HEADER_DXT10? headerDXT10 = null;
            if (bDXT10Header)
                headerDXT10 = (DDS_HEADER_DXT10)Marshal.PtrToStructure(ddsData + offset - sizeofDDS_HEADER_DXT10, typeof(DDS_HEADER_DXT10));

            Result hr = CreateTextureFromDDS(d3dDevice, null,
                    header, headerDXT10, ddsData + offset, ddsDataSize - offset, maxsize,
                usage, bindFlags, CpuAccessFlags, miscFlags, forceSRGB,
                out texture, out textureView);
            if (hr.Success)
            {
                texture.DebugName = "DDSTextureLoader";
                textureView.DebugName = "DDSTextureLoader";

                alphaMode = GetAlphaMode(header, ddsData + sizeofDDS_MAGIC);
            }
        }
        public static void CreateDDSTextureFromMemoryEx(Device d3dDevice,
            DeviceContext d3dContext,
    IntPtr ddsData,
            int ddsDataSize,
            int maxsize,
            ResourceUsage usage,
            BindFlags bindFlags,
            CpuAccessFlags CpuAccessFlags,
            ResourceOptionFlags miscFlags,
            bool forceSRGB,
            out Resource texture,
            out ShaderResourceView textureView,
            out DDS_ALPHA_MODE alphaMode)
        {
            texture = null;
            textureView = null;
            alphaMode = DDS_ALPHA_MODE.DDS_ALPHA_MODE_UNKNOWN;

            var sizeofDDS_HEADER = Marshal.SizeOf(typeof(DDS_HEADER));
            var sizeofDDS_MAGIC = sizeof(int);
            var sizeofDDS_PIXELFORMAT = Marshal.SizeOf(typeof(DDS_PIXELFORMAT));
            var sizeofDDS_HEADER_DXT10 = Marshal.SizeOf(typeof(DDS_HEADER_DXT10));

            if (d3dDevice == null)
                throw new ArgumentNullException("d3dDevice");
            if (ddsData == IntPtr.Zero)
                throw new ArgumentOutOfRangeException("ddsData");

            // Validate DDS file in memory
            if (ddsDataSize < (sizeof(int) + Marshal.SizeOf(typeof(DDS_HEADER))))
            {
                throw new ArgumentOutOfRangeException("ddsDataSize");
            }

            int dwMagicNumber = Marshal.ReadInt32(ddsData);
            if (dwMagicNumber != DDS_MAGIC)
            {
                throw new ArgumentException("Not a valid DDS", "ddsData");
            }

            var header = (DDS_HEADER)Marshal.PtrToStructure(ddsData + sizeofDDS_MAGIC, typeof(DDS_HEADER));

            // Verify header to validate DDS file
            if (header.size != sizeofDDS_HEADER ||
                header.ddspf.size != sizeofDDS_PIXELFORMAT)
            {
                throw new Exception("Invalid DDS");
            }

            // Check for DX10 extension
            bool bDXT10Header = false;
            if ((header.ddspf.flags & DDS_FOURCC) > 0 &&
                (MAKEFOURCC('D', 'X', '1', '0') == header.ddspf.fourCC))
            {
                // Must be long enough for both headers and magic value
                if (ddsDataSize < sizeofDDS_HEADER + sizeofDDS_MAGIC + sizeofDDS_HEADER_DXT10)
                {
                    throw new ArgumentOutOfRangeException("ddsDataSize");
                }

                bDXT10Header = true;
            }

            int offset = sizeofDDS_MAGIC
                + sizeofDDS_HEADER
                + (bDXT10Header ? sizeofDDS_HEADER_DXT10 : 0);
            DDS_HEADER_DXT10? headerDXT10 = null;
            if (bDXT10Header)
                headerDXT10 = (DDS_HEADER_DXT10)Marshal.PtrToStructure(ddsData + offset - sizeofDDS_HEADER_DXT10, typeof(DDS_HEADER_DXT10));


            Result hr = CreateTextureFromDDS(d3dDevice, d3dContext,
                header, headerDXT10, ddsData + offset, ddsDataSize - offset, maxsize,
                usage, bindFlags, CpuAccessFlags, miscFlags, forceSRGB,
                out texture, out textureView);
            if (hr.Success)
            {
                texture.DebugName = "DDSTextureLoader";

                textureView.DebugName = "DDSTextureLoader";

                alphaMode = GetAlphaMode(header, ddsData + sizeofDDS_MAGIC);
            }
        }

        //--------------------------------------------------------------------------------------
        public static void CreateDDSTextureFromFile(Device d3dDevice,
            string fileName,
            out Resource texture,
            out ShaderResourceView textureView,
            int maxsize,
            out DDS_ALPHA_MODE alphaMode)
        {
            CreateDDSTextureFromFileEx(d3dDevice, fileName, maxsize,
                ResourceUsage.Default, BindFlags.ShaderResource, 0, 0, false,
                out texture, out textureView, out alphaMode);
        }

        public static void CreateDDSTextureFromFile(Device d3dDevice,
            DeviceContext d3dContext,
            string fileName,
            out Resource texture,
            out ShaderResourceView textureView,
            int maxsize,
            out DDS_ALPHA_MODE alphaMode)
        {
            CreateDDSTextureFromFileEx(d3dDevice, d3dContext, fileName, maxsize,
                ResourceUsage.Default, BindFlags.ShaderResource, 0, 0, false,
                out texture, out textureView, out alphaMode);
        }

        public static void CreateDDSTextureFromFileEx(Device d3dDevice,
            string fileName,
            int maxsize,
            ResourceUsage usage,
            BindFlags bindFlags,
            CpuAccessFlags cpuAccessFlags,
            ResourceOptionFlags miscFlags,
            bool forceSRGB,
            out Resource texture,
            out ShaderResourceView textureView,
            out DDS_ALPHA_MODE alphaMode)
        {
            texture = null;
            textureView = null;
            alphaMode = DDS_ALPHA_MODE.DDS_ALPHA_MODE_UNKNOWN;

            if (d3dDevice == null)
                throw new ArgumentNullException("d3dDevice");
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentOutOfRangeException("fileName");
            if (!File.Exists(fileName))
                throw new FileNotFoundException("File does not exist", fileName);

            IntPtr ddsData = IntPtr.Zero;

            byte[] fileContents = File.ReadAllBytes(fileName);

            GCHandle handle = GCHandle.Alloc(fileContents, GCHandleType.Pinned);
            try
            {
                ddsData = handle.AddrOfPinnedObject();
                CreateDDSTextureFromMemoryEx(d3dDevice, ddsData, fileContents.Length, maxsize, usage, bindFlags, cpuAccessFlags, miscFlags, forceSRGB, out texture, out textureView, out alphaMode);
            }
            finally
            {
                handle.Free();
            }
#if DEBUG
            if (texture != null)
            {
                texture.DebugName = fileName;
            }
            if (textureView != null)
            {
                textureView.DebugName = fileName;
            }
#endif

        }

        public static void CreateDDSTextureFromFileEx(Device d3dDevice,
            DeviceContext d3dContext,
            string fileName,
            int maxsize,
            ResourceUsage usage,
            BindFlags bindFlags,
            CpuAccessFlags CpuAccessFlags,
            ResourceOptionFlags miscFlags,
            bool forceSRGB,
            out Resource texture,
            out ShaderResourceView textureView,
            out DDS_ALPHA_MODE alphaMode)
        {
            texture = null;
            textureView = null;
            alphaMode = DDS_ALPHA_MODE.DDS_ALPHA_MODE_UNKNOWN;

            if (d3dDevice == null)
                throw new ArgumentNullException("d3dDevice");
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentOutOfRangeException("fileName");
            if (!File.Exists(fileName))
                throw new FileNotFoundException("File does not exist", fileName);

            IntPtr ddsData = IntPtr.Zero;

            byte[] fileContents = File.ReadAllBytes(fileName);

            GCHandle handle = GCHandle.Alloc(fileContents, GCHandleType.Pinned);
            try
            {
                ddsData = handle.AddrOfPinnedObject();
                CreateDDSTextureFromMemoryEx(d3dDevice, d3dContext, ddsData, fileContents.Length, maxsize, usage, bindFlags, CpuAccessFlags, miscFlags, forceSRGB, out texture, out textureView, out alphaMode);
            }
            finally
            {
                handle.Free();
            }
#if DEBUG
            if (texture != null)
            {
                texture.DebugName = fileName;
            }
            if (textureView != null)
            {
                textureView.DebugName = fileName;
            }
#endif
        }
    }
}