using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class TextureArray2D : IGlObject
    {
        private int id = 0;
        private int cubeId = 0;
        private int[] tex2DId;

        private readonly SizedInternalFormat internalFormat;
        private readonly int nMipmaps;
        private readonly int nLayer;

        public TextureArray2D(int numLayers, int numMipmaps, SizedInternalFormat internalFormat, int width, int height)
        {
            id = GL.GenTexture();
            this.internalFormat = internalFormat;
            this.nMipmaps = numMipmaps;
            this.nLayer = numLayers;
            GL.BindTexture(TextureTarget.Texture2DArray, id);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, numMipmaps,
                internalFormat, width,
                height, numLayers);
            Utility.GLCheck();
        }

        public TextureArray2D(ImageLoader.Image image)
        {
            id = GL.GenTexture();
            this.nMipmaps = image.GetNumMipmaps();
            this.nLayer = image.Layers.Count;
            this.internalFormat = (SizedInternalFormat)image.OpenglInternalFormat;

            GL.BindTexture(TextureTarget.Texture2DArray, id);

            // create storage

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, image.GetNumMipmaps(),
                internalFormat, image.GetWidth(0),
                image.GetHeight(0), image.Layers.Count);
            Utility.GLCheck();

            if (image.IsCompressed)
            {
                for (int face = 0; face < image.Layers.Count; ++face)
                {
                    for (int level = 0; level < image.GetNumMipmaps(); ++level)
                    {
                        GL.CompressedTexSubImage3D(TextureTarget.Texture2DArray, level, 
                            0, 0, face, image.GetWidth(level), image.GetHeight(level),
                            1, (PixelFormat)image.OpenglInternalFormat, 
                            (int)image.Layers[face].Mipmaps[level].Size,
                            image.Layers[face].Mipmaps[level].Bytes);
                    }
                }
            }
            else
            {
                var pixelFormat = (PixelFormat) image.OpenglExternalFormat;
                var pixelType = (PixelType) image.OpenglType;

                for (int face = 0; face < image.Layers.Count; ++face)
                {
                    for (int level = 0; level < image.GetNumMipmaps(); ++level)
                    {
                        GL.TexSubImage3D(TextureTarget.Texture2DArray, level,
                            0, 0, face, image.GetWidth(level), image.GetHeight(level),
                            1, pixelFormat, pixelType, image.Layers[face].Mipmaps[level].Bytes);
                    }
                }
            }
            
            Utility.GLCheck();
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinLod, 0.0f);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLod, (float)image.GetNumMipmaps());
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBaseLevel, 0);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMaxLevel, image.GetNumMipmaps());
        }
        

        /// <summary>
        /// creates texture 2d views if they were not already created
        /// </summary>
        private void CreateTexture2DViews()
        {
            if(tex2DId != null)
                return;

            tex2DId = new int[nLayer * nMipmaps];
            GL.GenTextures(nLayer, tex2DId);
            for (int curLayer = 0; curLayer < nLayer; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
                {
                    GL.TextureView(tex2DId[GetTextureIndex(curLayer, curMipmap)], TextureTarget.Texture2D, id,
                        (PixelInternalFormat)internalFormat, curMipmap, 1, curLayer, 1);

                    Utility.GLCheck();
                    GL.BindTexture(TextureTarget.Texture2D, tex2DId[GetTextureIndex(curLayer, curMipmap)]);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
                    Utility.GLCheck();
                }
            }
        }

        private int GetTextureIndex(int layer, int level)
        {
            return layer * nLayer + level;
        }

        /// <summary>
        /// creates cube map view if it was not already created
        /// </summary>
        private void CreateCubeMapView()
        {
            if(cubeId != 0)
                return;

            Debug.Assert(nLayer == 6);
            cubeId = GL.GenTexture();
            GL.TextureView(cubeId, TextureTarget.TextureCubeMap, id,
                (PixelInternalFormat)internalFormat, 0, nMipmaps, 0, 6);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeId);

            Utility.GLCheck();

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToEdge);

            Utility.GLCheck();
        }

        /// <summary>
        /// binds specified texture
        /// </summary>
        /// <param name="slot">binding slot</param>
        /// <param name="target">texture target</param>
        /// <param name="texId">texture id</param>
        /// <param name="linearFiltering">linear filter or nearest neighbor</param>
        private void BindAs(int slot, TextureTarget target, int texId, bool linearFiltering)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            Utility.GLCheck();
            GL.BindTexture(target, texId);
            
            GL.TexParameter(target, TextureParameterName.TextureMinFilter, GetMinFilter(linearFiltering));
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, GetMagFilter(linearFiltering));
            Utility.GLCheck();
        }

        private int GetMinFilter(bool linearFiltering)
        {
            if (nMipmaps > 0)
            {
                if (linearFiltering) return (int) TextureMinFilter.LinearMipmapNearest; // sharps switching between mipmaps
                return (int) TextureMinFilter.NearestMipmapNearest;
            }
            if (linearFiltering) return (int) TextureMinFilter.Linear;
            return (int) TextureMinFilter.Nearest;
        }

        private int GetMagFilter(bool linearFiltering)
        {
            if (linearFiltering) return (int) TextureMagFilter.Linear;
            return (int) TextureMagFilter.Nearest;
        }

        /// <summary>
        /// binds texture as texture array 2d
        /// </summary>
        /// <param name="slot">binding slot</param>
        /// <param name="linearFiltering">linear filter or nearest neighbor</param>
        public void Bind(int slot, bool linearFiltering)
        {
            BindAs(slot, TextureTarget.Texture2DArray, id, linearFiltering);
        }

        /// <summary>
        /// binds texture as cube map
        /// </summary>
        /// <param name="slot">binding slot</param>
        /// <param name="linearFiltering">linear filter or nearest neighbor</param>
        public void BindAsCubemap(int slot, bool linearFiltering)
        {
            CreateCubeMapView();
            BindAs(slot, TextureTarget.TextureCubeMap, cubeId, linearFiltering);
        }

        /// <summary>
        /// binds texture as texture2D
        /// </summary>
        /// <param name="slot">binding slot</param>
        /// <param name="linearFiltering">linear filter or nearest neighbor</param>
        /// <param name="layer">which layer of the texture</param>
        /// <param name="level">mipmap level</param>
        public void BindAsTexture2D(int slot, bool linearFiltering, int layer, int level)
        {
            CreateTexture2DViews();
            BindAs(slot, TextureTarget.Texture2D, tex2DId[GetTextureIndex(layer, level)], linearFiltering);
        }

        /// <summary>
        /// binds the texture as image
        /// </summary>
        /// <param name="slot">binding slot</param>
        /// <param name="level">which level</param>
        /// <param name="layer">which layer</param>
        /// <param name="access">texture access</param>
        public void BindAsImage(int slot, int level, int layer, TextureAccess access)
        {
            GL.BindImageTexture(slot, id, level, false, layer, access, internalFormat);
        }

        /// <summary>
        /// reads data from gpu
        /// </summary>
        /// <param name="level">requested level</param>
        /// <param name="layer">requested layer or -1 if all layers</param>
        /// <param name="format"></param>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public byte[] GetData(int level, int layer, PixelFormat format, PixelType type, out int width, out int height)
        {
            // retrieve width and height of the level
            GL.BindTexture(TextureTarget.Texture2DArray, id);
            GL.GetTexLevelParameter(TextureTarget.Texture2DArray, level, GetTextureParameter.TextureWidth, out width);
            GL.GetTexLevelParameter(TextureTarget.Texture2DArray, level, GetTextureParameter.TextureHeight, out height);

            Debug.Assert((uint)layer < nLayer);

            int bufferSize = width * height * GetPixelTypeSize(type) * GetPixelFormatCount(format) * nLayer;
            byte[] buffer = new byte[bufferSize];

            Utility.ReadTexture(id, level, format, type, ref buffer, 0, 0, width, height * nLayer);

            if (nLayer > 1 && layer >= 0)
            {
                // get data from the layer
                int layerSize = bufferSize / nLayer;
                byte[] layerBuffer = new byte[layerSize];
                for (int i = 0; i < layerSize; ++i)
                    layerBuffer[i] = buffer[layer * layerSize + i];

                buffer = layerBuffer;
            }

            // mirror horizontally
            if (layer >= 0)
            {
                // only this layer
                MirrorHorizontally(buffer, width * GetPixelTypeSize(type) * GetPixelFormatCount(format), height, layer * (bufferSize / nLayer));
            }
            else
            {
                // all layer
                for(int curLayer = 0; curLayer < nLayer; ++curLayer)
                    MirrorHorizontally(buffer, width * GetPixelTypeSize(type) * GetPixelFormatCount(format), height, curLayer * (bufferSize / nLayer));
            }

            return buffer;
        }

        private void MirrorHorizontally(byte[] buffer, int lineWidth, int height, int offset)
        {
            for (int y = 0; y < height / 2; ++y)
            { 
                for (int x = 0; x < lineWidth; ++x)
                {
                    var a = y * lineWidth + x + offset;
                    var b = (height - y - 1) * lineWidth + x + offset;
                    var tmp = buffer[a];
                    buffer[a] = buffer[b];
                    buffer[b] = tmp;
                }
            }
        }

        public static int GetPixelFormatCount(PixelFormat f)
        {
            switch (f)
            {
                case PixelFormat.RedInteger:
                case PixelFormat.GreenInteger:
                case PixelFormat.BlueInteger:
                case PixelFormat.AlphaInteger:
                case PixelFormat.Green:
                case PixelFormat.Blue:
                case PixelFormat.Alpha:
                case PixelFormat.Red:
                case PixelFormat.Luminance:
                    return 1;

                case PixelFormat.Rgb:
                case PixelFormat.Bgr:
                case PixelFormat.RgbInteger:
                case PixelFormat.BgrInteger:
                    return 3;
                case PixelFormat.Rgba:
                case PixelFormat.AbgrExt:
                case PixelFormat.Bgra:
                case PixelFormat.RgbaInteger:
                case PixelFormat.BgraInteger:
                    return 4;
                case PixelFormat.RgInteger:
                case PixelFormat.Rg:
                case PixelFormat.LuminanceAlpha:
                    return 2;
                default:
                    throw new Exception("invalid pixel format used: " + f);
            }
        }

        private static int GetPixelTypeSize(PixelType t)
        {
            switch (t)
            {
                case PixelType.UnsignedByte:
                case PixelType.Byte:
                    return 1;
                case PixelType.HalfFloat:
                case PixelType.Short:
                case PixelType.UnsignedShort:
                    return 2;
                case PixelType.Int:
                case PixelType.UnsignedInt:
                case PixelType.Float:
                    return 4;
                default:
                    throw new Exception("invalid pixel type used: " + t);
            }
        }

        public void Dispose()
        {
            if (tex2DId != null)
            {
                GL.DeleteTextures(nLayer * nMipmaps, tex2DId);
                tex2DId = null;
            }
            if (cubeId != 0)
            {
                GL.DeleteTexture(cubeId);
                cubeId = 0;
            }
            if (id != 0)
            {
                GL.DeleteTexture(id);
                id = 0;
            }
        }
    }
}
