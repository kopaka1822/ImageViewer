using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.DirectX
{
    public class TextureArray2D : IDisposable
    {
        public int Width { get; }
        public int Height { get; }
        public int NumMipmaps { get; }
        public bool HasMipmaps => NumMipmaps > 1;
        public int NumLayers { get; }
        public ShaderResourceView View { get; private set; }

        public Format Format { get; }

        private readonly Texture2D handle;
        private ShaderResourceView[] views;
        private ShaderResourceView[] cubeViews;
        private RenderTargetView[] rtViews;
        private UnorderedAccessView[] uaViews;

        public TextureArray2D(int numLayer, int numMipmaps, int width, int height, Format format, bool createUav)
        {
            Width = width;
            Height = height;
            NumMipmaps = numMipmaps;
            NumLayers = numLayer;
            this.Format = format;

           
            handle = new SharpDX.Direct3D11.Texture2D(Device.Get().Handle, CreateTextureDescription(createUav));

            CreateTextureViews(createUav);
        }

        public TextureArray2D(ImageLoader.Image image)
        {
            Width = image.GetWidth(0);
            Height = image.GetHeight(0);
            NumMipmaps = image.NumMipmaps;
            NumLayers = image.NumLayers;
            Format = image.Format.DxgiFormat;

            var data = new DataRectangle[NumLayers * NumMipmaps];
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
                {
                    var mip = image.Layers[curLayer].Mipmaps[curMipmap];
                    var idx = GetTextureIndex(curLayer, curMipmap);
                    data[idx].DataPointer = mip.Bytes;
                    // The distance (in bytes) from the beginning of one line of a texture to the next line.
                    data[idx].Pitch = (int)(mip.Size / mip.Height);
                }
            }

            handle = new Texture2D(Device.Get().Handle, CreateTextureDescription(false), data);

            CreateTextureViews(false);
        }

        public ShaderResourceView GetSrView(int layer, int mipmap)
        {
            return views[GetTextureIndex(layer, mipmap)];
        }

        public ShaderResourceView GetCubeView(int mipmap)
        {
            Debug.Assert(cubeViews != null);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(mipmap >= 0);
            return cubeViews[mipmap];
        }

        public RenderTargetView GetRtView(int layer, int mipmap)
        {
            return rtViews[GetTextureIndex(layer, mipmap)];
        }

        public UnorderedAccessView GetUaView(int mipmap)
        {
            Debug.Assert(uaViews != null);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(mipmap >= 0);
            return uaViews[mipmap];
        }

        /// <summary>
        /// generates new mipmaps
        /// </summary>
        public TextureArray2D GenerateMipmapLevels(int levels)
        {
            Debug.Assert(!HasMipmaps);
            var newTex = new TextureArray2D(NumLayers, levels, Width, Height, Format, uaViews != null);

            // copy all layers
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                // copy image data of first level
                Device.Get().CopySubresource(handle, newTex.handle, GetTextureIndex(curLayer, 0), newTex.GetTextureIndex(curLayer, 0), Width, Height);
            }

            Device.Get().GenerateMips(newTex.View);

            return newTex;
        }

        /// <summary>
        /// creates a new texture that has only one mipmap level
        /// </summary>
        /// <returns></returns>
        public TextureArray2D CloneWithoutMipmaps(int mipmap = 0)
        {
            var newTex = new TextureArray2D(NumLayers, 1, GetWidth(mipmap), GetHeight(mipmap), Format, uaViews != null);

            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                // copy data of first level
                Device.Get().CopySubresource(handle, newTex.handle, GetTextureIndex(curLayer, mipmap), newTex.GetTextureIndex(curLayer, 0), Width, Height);
            }

            return newTex;
        }

        /// <summary>
        /// performs a deep gpu copy of the textures
        /// </summary>
        /// <returns></returns>
        public TextureArray2D Clone()
        {
            var newTex = new TextureArray2D(NumLayers, NumMipmaps, Width, Height, Format, uaViews != null);

            Device.Get().CopyResource(handle, newTex.handle);
            return newTex;
        }

        public Color[] GetPixelColors(int layer, int mipmap)
        {
            // create staging texture
            using (var staging = GetStagingTexture(layer, mipmap))
            {
                // obtain data from staging resource
                return Device.Get().GetColorData(staging, 0, GetWidth(mipmap), GetHeight(mipmap));
            }
        }

        public unsafe byte[] GetBytes(int layer, int mipmap, uint size)
        {
            byte[] res = new byte[size];
            fixed (byte* ptr = res)
            {
                CopyPixels(layer, mipmap, new IntPtr(ptr), size);
            }

            return res;
        }

        public void CopyPixels(int layer, int mipmap, IntPtr destination, uint size)
        {
            // create staging texture
            using (var staging = GetStagingTexture(layer, mipmap))
            {
                // obtain data from staging resource
                Device.Get().GetData(staging, 0, GetWidth(mipmap), GetHeight(mipmap), destination, size);
            }
        }

        private Texture2D GetStagingTexture(int layer, int mipmap)
        {
            Debug.Assert(IO.SupportedFormats.Contains(Format));

            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(layer >= 0);
            Debug.Assert(layer < NumLayers);

            var desc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Format = Format,
                Height = GetHeight(mipmap),
                Width = GetWidth(mipmap),
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging
            };

            // create staging texture
            var staging = new Texture2D(Device.Get().Handle, desc);

            // copy data to staging resource
            Device.Get().CopySubresource(handle, staging,
                GetTextureIndex(layer, mipmap), 0,
                GetWidth(mipmap), GetHeight(mipmap)
            );

            return staging;
        }

        public int GetWidth(int mipmap)
        {
            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            return Math.Max(1, Width >> mipmap);
        }

        public int GetHeight(int mipmap)
        {
            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            return Math.Max(1, Height >> mipmap);
        }

        public void Dispose()
        {
            if (views != null)
            {
                foreach (var shaderResourceView in views)
                {
                    shaderResourceView?.Dispose();
                }
            }

            if (cubeViews != null)
            {
                foreach (var view in cubeViews)
                {
                    view?.Dispose();
                }
            }

            if (rtViews != null)
            {
                foreach (var renderTargetView in rtViews)
                {
                    renderTargetView?.Dispose();
                }
            }

            if (uaViews != null)
            {
                foreach (var unorderedAccessView in uaViews)
                {
                    unorderedAccessView?.Dispose();
                }
            }

            View?.Dispose();
            handle?.Dispose();
        }

        private void CreateTextureViews(bool createUav)
        {
            Debug.Assert(handle != null);

            // default View
            var defaultDesc = new ShaderResourceViewDescription
            {
                Dimension = ShaderResourceViewDimension.Texture2DArray,
                Format = Format,
                Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                {
                    ArraySize = NumLayers,
                    FirstArraySlice = 0,
                    MipLevels = NumMipmaps,
                    MostDetailedMip = 0
                }
            };
            View = new ShaderResourceView(Device.Get().Handle, handle, defaultDesc);

            if (NumLayers == 6)
            {
                cubeViews = new ShaderResourceView[NumMipmaps];
                for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
                {
                    var cubeDesc = new ShaderResourceViewDescription
                    {
                        Dimension = ShaderResourceViewDimension.TextureCube,
                        Format = Format,
                        TextureCube = new ShaderResourceViewDescription.TextureCubeResource
                        {
                            MipLevels = 1,
                            MostDetailedMip = curMipmap
                        }
                    };

                    cubeViews[curMipmap] = new ShaderResourceView(Device.Get().Handle, handle, cubeDesc);
                }
            }

            // single slice views
            views = new ShaderResourceView[NumLayers * NumMipmaps];
            rtViews = new RenderTargetView[NumLayers * NumMipmaps];
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
                {
                    var desc = new ShaderResourceViewDescription
                    {
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Format = Format,
                        Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                        {
                            MipLevels = 1,
                            MostDetailedMip = curMipmap,
                            ArraySize = 1,
                            FirstArraySlice = curLayer
                        }
                    };

                    views[GetTextureIndex(curLayer, curMipmap)] = new ShaderResourceView(Device.Get().Handle, handle, desc);

                    var rtDesc = new RenderTargetViewDescription
                    {
                        Dimension = RenderTargetViewDimension.Texture2DArray,
                        Format = Format,
                        Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource
                        {
                            ArraySize = 1,
                            FirstArraySlice = curLayer,
                            MipSlice = curMipmap
                        }
                    };

                    rtViews[GetTextureIndex(curLayer, curMipmap)] = new RenderTargetView(Device.Get().Handle, handle, rtDesc);
                }
            }

            if (createUav)
            {
                uaViews = new UnorderedAccessView[NumMipmaps];
                for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
                {
                    var desc = new UnorderedAccessViewDescription
                    {
                        Dimension = UnorderedAccessViewDimension.Texture2DArray,
                        Format = Format,
                        Texture2DArray = new UnorderedAccessViewDescription.Texture2DArrayResource
                        {
                            ArraySize = NumLayers,
                            FirstArraySlice = 0,
                            MipSlice = curMipmap
                        }
                    };

                    uaViews[curMipmap] = new UnorderedAccessView(Device.Get().Handle, handle, desc);
                }
            }
        }

        private int GetTextureIndex(int layer, int mipmap)
        {
            Debug.Assert(layer < NumLayers);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(layer >= 0);
            Debug.Assert(mipmap >= 0);

            return layer * NumMipmaps + mipmap;
        }

        private Texture2DDescription CreateTextureDescription(bool createUav)
        {
            Debug.Assert(NumLayers > 0);
            Debug.Assert(NumMipmaps > 0);
            Debug.Assert(Width > 0);
            Debug.Assert(Height > 0);
            Debug.Assert(Format != Format.Unknown);

            // render target required for mip map generation
            BindFlags flags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            if (createUav)
                flags |= BindFlags.UnorderedAccess;

            var optionFlags = ResourceOptionFlags.GenerateMipMaps;
            if (NumLayers == 6)
                optionFlags |= ResourceOptionFlags.TextureCube;

            return new Texture2DDescription
            {
                ArraySize = NumLayers,
                BindFlags = flags,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format,
                Height = Height,
                MipLevels = NumMipmaps,
                OptionFlags = optionFlags,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = Width
            };
        }
    }
}
