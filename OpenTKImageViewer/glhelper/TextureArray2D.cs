using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKImageViewer.glhelper
{
    public class TextureArray2D
    {
        private int id;
        private int cubeId = 0;
        private readonly SizedInternalFormat internalFormat;

        public TextureArray2D(int numLayers, int numMipmaps, SizedInternalFormat internalFormat, int width, int height)
        {
            id = GL.GenTexture();
            this.internalFormat = internalFormat;
            GL.BindTexture(TextureTarget.Texture2DArray, id);

            GL.TexStorage3D(TextureTarget3d.Texture2DArray, numMipmaps,
                internalFormat, width,
                height, numLayers);
            Utility.GLCheck();

            if(numLayers == 6)
                CreateCubeMapView(numMipmaps, (PixelInternalFormat)internalFormat);
        }

        public TextureArray2D(ImageLoader.Image image)
        {
            id = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, id);

            // create storage
            internalFormat = (SizedInternalFormat)image.OpenglInternalFormat;

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

        private void CreateCubeMapView(int numLevels, PixelInternalFormat pixelInternalFormat)
        {
            cubeId = GL.GenTexture();
            GL.TextureView(cubeId, TextureTarget.TextureCubeMap, id,
                pixelInternalFormat, 0, numLevels, 0, 6);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeId);

            Utility.GLCheck();

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToEdge);

            Utility.GLCheck();
        }

        private void BindAs(int slot, TextureTarget target, int texId)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + slot);
            Utility.GLCheck();
            GL.BindTexture(target, texId);
            GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            Utility.GLCheck();
        }

        public void Bind(int slot)
        {
            BindAs(slot, TextureTarget.Texture2DArray, id);
        }

        public void BindAsCubemap(int slot)
        {
            BindAs(slot, TextureTarget.TextureCubeMap, cubeId);
        }

        public void BindAsImage(int slot, int level, int layer, TextureAccess access)
        {
            GL.BindImageTexture(slot, id, level, false, layer, access, internalFormat);
        }
    }
}
