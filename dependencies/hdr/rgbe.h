/*M///////////////////////////////////////////////////////////////////////////////////////
//
//  IMPORTANT: READ BEFORE DOWNLOADING, COPYING, INSTALLING OR USING.
//
//  By downloading, copying, installing or using the software you agree to this license.
//  If you do not agree to this license, do not download, install,
//  copy or use the software.
//
//
//                           License Agreement
//                For Open Source Computer Vision Library
//
// Copyright (C) 2000-2008, Intel Corporation, all rights reserved.
// Copyright (C) 2009, Willow Garage Inc., all rights reserved.
// Third party copyrights are property of their respective owners.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
//   * Redistribution's of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//
//   * Redistribution's in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//
//   * The name of the copyright holders may not be used to endorse or promote products
//     derived from this software without specific prior written permission.
//
// This software is provided by the copyright holders and contributors "as is" and
// any express or implied warranties, including, but not limited to, the implied
// warranties of merchantability and fitness for a particular purpose are disclaimed.
// In no event shall the Intel Corporation or contributors be liable for any direct,
// indirect, incidental, special, exemplary, or consequential damages
// (including, but not limited to, procurement of substitute goods or services;
// loss of use, data, or profits; or business interruption) however caused
// and on any theory of liability, whether in contract, strict liability,
// or tort (including negligence or otherwise) arising in any way out of
// the use of this software, even if advised of the possibility of such damage.
//
//M*/
// posted to http://www.graphics.cornell.edu/~bjw/
// written by Bruce Walter  (bjw@graphics.cornell.edu)  5/26/95
// based on code written by Greg Ward

#include <sstream>
#include <stdio.h>

typedef struct {
    int valid;            /* indicate which fields are valid */
    char programtype[16]; /* listed at beginning of file to identify it
                           * after "#?".  defaults to "RGBE" */
    float gamma;          /* image has already been gamma corrected with
                           * given gamma.  defaults to 1.0 (no correction) */
    float exposure;       /* a value of 1.0 in an image corresponds to
         * <exposure> watts/steradian/m^2.
         * defaults to 1.0 */
} rgbe_header_info;

/* flags indicating which fields in an rgbe_header_info are valid */
#define RGBE_VALID_PROGRAMTYPE 0x01
#define RGBE_VALID_GAMMA       0x02
#define RGBE_VALID_EXPOSURE    0x04

/* return codes for rgbe routines */
#define RGBE_RETURN_SUCCESS 0
#define RGBE_RETURN_FAILURE -1

/* read or write headers */
/* you may set rgbe_header_info to null if you want to */
void RGBE_WriteHeader(FILE* fp, int width, int height, rgbe_header_info* info);
void RGBE_ReadHeader(FILE* fp, int* width, int* height, rgbe_header_info* info);

/* read or write pixels */
/* can read or write pixels in chunks of any size including single pixels*/
void RGBE_WritePixels(FILE* fp, float* data, int numpixels);
void RGBE_ReadPixels(FILE* fp, float* data, int numpixels);

/* read or write run length encoded files */
/* must be called to read or write whole scanlines */
void RGBE_WritePixels_RLE(FILE* fp, float* data, int scanline_width,
    int num_scanlines);
void RGBE_ReadPixels_RLE(FILE* fp, float* data, int scanline_width,
    int num_scanlines);









// inlined source file below:


#include <math.h>
#if !defined(__APPLE__)
#include <malloc.h>
#endif
#include <string.h>
#include <ctype.h>

// This file contains code to read and write four byte rgbe file format
// developed by Greg Ward.  It handles the conversions between rgbe and
// pixels consisting of floats.  The data is assumed to be an array of floats.
// By default there are three floats per pixel in the order red, green, blue.
// (RGBE_DATA_??? values control this.)  Only the mimimal header reading and
// writing is implemented.  Each routine does error checking and will return
// a status value as defined below.  This code is intended as a skeleton so
// feel free to modify it to suit your needs.

// Some opencv specific changes have been added:
// inline define specified, error handler uses CV_Error,
// defines changed to work in bgr color space.
//
// posted to http://www.graphics.cornell.edu/~bjw/
// written by Bruce Walter  (bjw@graphics.cornell.edu)  5/26/95
// based on code written by Greg Ward

#define INLINE inline

/* offsets to red, green, and blue components in a data (float) pixel */
#define RGBE_DATA_RED    2
#define RGBE_DATA_GREEN  1
#define RGBE_DATA_BLUE   0
/* number of floats per pixel */
#define RGBE_DATA_SIZE   3

enum rgbe_error_codes {
    rgbe_read_error,
    rgbe_write_error,
    rgbe_format_error,
    rgbe_memory_error
};

/* default error routine.  change this to change error handling */
static std::runtime_error rgbe_error(int rgbe_error_code, const char* msg)
{
    std::stringstream errmsg;
    switch (rgbe_error_code) {
    case rgbe_read_error:
        errmsg << "RGBE read error";
        break;
    case rgbe_write_error:
        errmsg << "RGBE write error";
        break;
    case rgbe_format_error:
        errmsg << "RGBE bad file format";
        break;
    case rgbe_memory_error:
    default:
        errmsg << "RGBE error:";
    }
    if(msg != nullptr)
    {
        errmsg << ": " << msg;
    }

    return std::runtime_error(errmsg.str());
}

/* standard conversion from float pixels to rgbe pixels */
/* note: you can remove the "inline"s if your compiler complains about it */
static INLINE void
float2rgbe(unsigned char rgbe[4], float red, float green, float blue)
{
    float v;
    int e;

    v = red;
    if (green > v) v = green;
    if (blue > v) v = blue;
    if (v < 1e-32) {
        rgbe[0] = rgbe[1] = rgbe[2] = rgbe[3] = 0;
    }
    else {
        v = static_cast<float>(frexp(v, &e) * 256.0 / v);
        // the order of rgb seems to be swapped
        rgbe[2] = (unsigned char)(red * v);
        rgbe[1] = (unsigned char)(green * v);
        rgbe[0] = (unsigned char)(blue * v);
        rgbe[3] = (unsigned char)(e + 128);
    }
}

/* standard conversion from rgbe to float pixels */
/* note: Ward uses ldexp(col+0.5,exp-(128+8)).  However we wanted pixels */
/*       in the range [0,1] to map back into the range [0,1].            */
static INLINE void
rgbe2float(float* red, float* green, float* blue, unsigned char rgbe[4])
{
    float f;

    if (rgbe[3]) {   /*nonzero pixel*/
        f = static_cast<float>(ldexp(1.0, rgbe[3] - (int)(128 + 8)));
        // the order of rgb seems to be swapped
        *red = rgbe[2] * f;
        *green = rgbe[1] * f;
        *blue = rgbe[0] * f;
    }
    else
        *red = *green = *blue = 0.0;
}

/* default minimal header. modify if you want more information in header */
void RGBE_WriteHeader(FILE* fp, int width, int height, rgbe_header_info* info)
{
    const char* programtype = "RGBE";

    if (info && (info->valid & RGBE_VALID_PROGRAMTYPE))
        programtype = info->programtype;
    if (fprintf(fp, "#?%s\n", programtype) < 0)
        throw rgbe_error(rgbe_write_error, NULL);
    /* The #? is to identify file type, the programtype is optional. */
    if (info && (info->valid & RGBE_VALID_GAMMA)) {
        if (fprintf(fp, "GAMMA=%g\n", info->gamma) < 0)
            throw rgbe_error(rgbe_write_error, NULL);
    }
    if (info && (info->valid & RGBE_VALID_EXPOSURE)) {
        if (fprintf(fp, "EXPOSURE=%g\n", info->exposure) < 0)
            throw rgbe_error(rgbe_write_error, NULL);
    }
    if (fprintf(fp, "FORMAT=32-bit_rle_rgbe\n\n") < 0)
        throw rgbe_error(rgbe_write_error, NULL);
    if (fprintf(fp, "-Y %d +X %d\n", height, width) < 0)
        throw rgbe_error(rgbe_write_error, NULL);
}

/* minimal header reading.  modify if you want to parse more information */
void RGBE_ReadHeader(FILE* fp, int* width, int* height, rgbe_header_info* info)
{
    char buf[128];
    float tempf;
    int i;

    if (info) {
        info->valid = 0;
        info->programtype[0] = 0;
        info->gamma = info->exposure = 1.0;
    }
    if (fgets(buf, sizeof(buf) / sizeof(buf[0]), fp) == NULL)
        throw rgbe_error(rgbe_read_error, NULL);
    if ((buf[0] != '#') || (buf[1] != '?')) {
        /* if you want to require the magic token then uncomment the next line */
        throw rgbe_error(rgbe_format_error,"bad initial token");
    }
    else if (info) {
        info->valid |= RGBE_VALID_PROGRAMTYPE;
        for (i = 0; i < static_cast<int>(sizeof(info->programtype) - 1); i++) {
            if ((buf[i + 2] == 0) || isspace(buf[i + 2]))
                break;
            info->programtype[i] = buf[i + 2];
        }
        info->programtype[i] = 0;
        if (fgets(buf, sizeof(buf) / sizeof(buf[0]), fp) == 0)
            throw rgbe_error(rgbe_read_error, NULL);
    }

    bool valid = false;
    for (;;) {
        if ((buf[0] == 0) || (buf[0] == '\n'))
        {
            if (valid) break;
            throw rgbe_error(rgbe_format_error, "no supported FORMAT specifier found (only supports 32-bit_rle_rgbe)");
        }
        else if (strcmp(buf, "FORMAT=32-bit_rle_rgbe\n") == 0)
        {
            valid = true; // supported format found
        }
        else if (info && (sscanf(buf, "GAMMA=%g", &tempf) == 1)) {
            info->gamma = tempf;
            info->valid |= RGBE_VALID_GAMMA;
        }
        else if (info && (sscanf(buf, "EXPOSURE=%g", &tempf) == 1)) {
            info->exposure = tempf;
            info->valid |= RGBE_VALID_EXPOSURE;
        }
        if (fgets(buf, sizeof(buf) / sizeof(buf[0]), fp) == 0)
            throw rgbe_error(rgbe_read_error, NULL);
    }
    //if (fgets(buf, sizeof(buf) / sizeof(buf[0]), fp) == 0)
    //    throw rgbe_error(rgbe_read_error, NULL);
    //if (strcmp(buf, "\n") != 0)
    //    throw rgbe_error(rgbe_format_error,
    //        "missing blank line after FORMAT specifier");
    if (fgets(buf, sizeof(buf) / sizeof(buf[0]), fp) == 0)
        throw rgbe_error(rgbe_read_error, NULL);
    if (sscanf(buf, "-Y %d +X %d", height, width) < 2)
        throw rgbe_error(rgbe_format_error, "missing image size specifier");
}

/* simple write routine that does not use run length encoding */
/* These routines can be made faster by allocating a larger buffer and
   fread-ing and fwrite-ing the data in larger chunks */
void RGBE_WritePixels(FILE* fp, float* data, int numpixels)
{
    unsigned char rgbe[4];

    while (numpixels-- > 0) {
        float2rgbe(rgbe, data[RGBE_DATA_RED],
            data[RGBE_DATA_GREEN], data[RGBE_DATA_BLUE]);
        data += RGBE_DATA_SIZE;
        if (fwrite(rgbe, sizeof(rgbe), 1, fp) < 1)
            throw rgbe_error(rgbe_write_error, NULL);
    }
}

/* simple read routine.  will not correctly handle run length encoding */
void RGBE_ReadPixels(FILE* fp, float* data, int numpixels)
{
    unsigned char rgbe[4];

    while (numpixels-- > 0) {
        if (fread(rgbe, sizeof(rgbe), 1, fp) < 1)
            throw rgbe_error(rgbe_read_error, NULL);
        rgbe2float(&data[RGBE_DATA_RED], &data[RGBE_DATA_GREEN],
            &data[RGBE_DATA_BLUE], rgbe);
        data += RGBE_DATA_SIZE;
    }
}

/* The code below is only needed for the run-length encoded files. */
/* Run length encoding adds considerable complexity but does */
/* save some space.  For each scanline, each channel (r,g,b,e) is */
/* encoded separately for better compression. */

static void RGBE_WriteBytes_RLE(FILE* fp, unsigned char* data, int numbytes)
{
#define MINRUNLENGTH 4
    int cur, beg_run, run_count, old_run_count, nonrun_count;
    unsigned char buf[2];

    cur = 0;
    while (cur < numbytes) {
        beg_run = cur;
        /* find next run of length at least 4 if one exists */
        run_count = old_run_count = 0;
        while ((run_count < MINRUNLENGTH) && (beg_run < numbytes)) {
            beg_run += run_count;
            old_run_count = run_count;
            run_count = 1;
            while ((beg_run + run_count < numbytes) && (run_count < 127)
                && (data[beg_run] == data[beg_run + run_count]))
                run_count++;
        }
        /* if data before next big run is a short run then write it as such */
        if ((old_run_count > 1) && (old_run_count == beg_run - cur)) {
            buf[0] = static_cast<unsigned char>(128 + old_run_count);   /*write short run*/
            buf[1] = data[cur];
            if (fwrite(buf, sizeof(buf[0]) * 2, 1, fp) < 1)
                throw rgbe_error(rgbe_write_error, NULL);
            cur = beg_run;
        }
        /* write out bytes until we reach the start of the next run */
        while (cur < beg_run) {
            nonrun_count = beg_run - cur;
            if (nonrun_count > 128)
                nonrun_count = 128;
            buf[0] = static_cast<unsigned char>(nonrun_count);
            if (fwrite(buf, sizeof(buf[0]), 1, fp) < 1)
                throw rgbe_error(rgbe_write_error, NULL);
            if (fwrite(&data[cur], sizeof(data[0]) * nonrun_count, 1, fp) < 1)
                throw rgbe_error(rgbe_write_error, NULL);
            cur += nonrun_count;
        }
        /* write out next run if one was found */
        if (run_count >= MINRUNLENGTH) {
            buf[0] = static_cast<unsigned char>(128 + run_count);
            buf[1] = data[beg_run];
            if (fwrite(buf, sizeof(buf[0]) * 2, 1, fp) < 1)
                throw rgbe_error(rgbe_write_error, NULL);
            cur += run_count;
        }
    }
#undef MINRUNLENGTH
}

void RGBE_WritePixels_RLE(FILE* fp, float* data, int scanline_width,
    int num_scanlines)
{
    unsigned char rgbe[4];
    std::unique_ptr<unsigned char[]> buffer;
    int i;

    if ((scanline_width < 8) || (scanline_width > 0x7fff))
        /* run length encoding is not allowed so write flat*/
        return RGBE_WritePixels(fp, data, scanline_width * num_scanlines);
    buffer.reset(new unsigned char[4 * scanline_width]);
    if (buffer == NULL)
        /* no buffer space so write flat */
        return RGBE_WritePixels(fp, data, scanline_width * num_scanlines);
    while (num_scanlines-- > 0) {
        rgbe[0] = 2;
        rgbe[1] = 2;
        rgbe[2] = static_cast<unsigned char>(scanline_width >> 8);
        rgbe[3] = scanline_width & 0xFF;
        if (fwrite(rgbe, sizeof(rgbe), 1, fp) < 1) {
            throw rgbe_error(rgbe_write_error, NULL);
        }
        for (i = 0; i < scanline_width; i++) {
            float2rgbe(rgbe, data[RGBE_DATA_RED],
                data[RGBE_DATA_GREEN], data[RGBE_DATA_BLUE]);
            buffer[i] = rgbe[0];
            buffer[i + scanline_width] = rgbe[1];
            buffer[i + 2 * scanline_width] = rgbe[2];
            buffer[i + 3 * scanline_width] = rgbe[3];
            data += RGBE_DATA_SIZE;
        }
        /* write out each of the four channels separately run length encoded */
        /* first red, then green, then blue, then exponent */
        for (i = 0; i < 4; i++) {
            RGBE_WriteBytes_RLE(fp, &buffer[i * scanline_width], scanline_width);
        }
    }
}

void RGBE_ReadPixels_RLE(FILE* fp, float* data, int scanline_width,
    int num_scanlines)
{
    unsigned char rgbe[4], * ptr, * ptr_end;
    int i, count;
    unsigned char buf[2];

    if ((scanline_width < 8) || (scanline_width > 0x7fff))
    {
        /* run length encoding is not allowed so read flat*/
        RGBE_ReadPixels(fp, data, scanline_width * num_scanlines);
        return;
    }

    std::unique_ptr<unsigned char[]> scanline_buffer;

    /* read in each successive scanline */
    while (num_scanlines > 0) {
        if (fread(rgbe, sizeof(rgbe), 1, fp) < 1) {
            throw rgbe_error(rgbe_read_error, NULL);
        }
        if ((rgbe[0] != 2) || (rgbe[1] != 2) || (rgbe[2] & 0x80)) {
            /* this file is not run length encoded */
            rgbe2float(&data[RGBE_DATA_RED], &data[RGBE_DATA_GREEN], &data[RGBE_DATA_BLUE], rgbe);
            data += RGBE_DATA_SIZE;
            scanline_buffer.reset();
            RGBE_ReadPixels(fp, data, scanline_width * num_scanlines - 1);
            return;
        }
        if ((((int)rgbe[2]) << 8 | rgbe[3]) != scanline_width) {
            throw rgbe_error(rgbe_format_error, "wrong scanline width");
        }
        if (scanline_buffer == NULL)
            scanline_buffer.reset(new unsigned char[4 * scanline_width]);
        if (scanline_buffer == NULL)
            throw rgbe_error(rgbe_memory_error, "unable to allocate buffer space");

        ptr = &scanline_buffer[0];
        /* read each of the four channels for the scanline into the buffer */
        for (i = 0; i < 4; i++) {
            ptr_end = &scanline_buffer[(i + 1) * scanline_width];
            while (ptr < ptr_end) {
                if (fread(buf, sizeof(buf[0]) * 2, 1, fp) < 1) {
                    throw rgbe_error(rgbe_read_error, NULL);
                }
                if (buf[0] > 128) {
                    /* a run of the same value */
                    count = buf[0] - 128;
                    if ((count == 0) || (count > ptr_end - ptr)) {
                        throw rgbe_error(rgbe_format_error, "bad scanline data");
                    }
                    while (count-- > 0)
                        *ptr++ = buf[1];
                }
                else {
                    /* a non-run */
                    count = buf[0];
                    if ((count == 0) || (count > ptr_end - ptr)) {
                        throw rgbe_error(rgbe_format_error, "bad scanline data");
                    }
                    *ptr++ = buf[1];
                    if (--count > 0) {
                        if (fread(ptr, sizeof(*ptr) * count, 1, fp) < 1) {
                            throw rgbe_error(rgbe_read_error, NULL);
                        }
                        ptr += count;
                    }
                }
            }
        }
        /* now convert data from buffer into floats */
        for (i = 0; i < scanline_width; i++) {
            rgbe[0] = scanline_buffer[i];
            rgbe[1] = scanline_buffer[i + scanline_width];
            rgbe[2] = scanline_buffer[i + 2 * scanline_width];
            rgbe[3] = scanline_buffer[i + 3 * scanline_width];
            rgbe2float(&data[RGBE_DATA_RED], &data[RGBE_DATA_GREEN],
                &data[RGBE_DATA_BLUE], rgbe);
            data += RGBE_DATA_SIZE;
        }
        num_scanlines--;
    }
}