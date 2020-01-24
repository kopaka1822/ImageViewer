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
using Resource = ImageFramework.ImageLoader.Resource;

namespace ImageFramework.DirectX
{
    public class TextureArray2D : TextureBase<TextureArray2D>
    {
        private readonly Texture2D handle;
        private ShaderResourceView[] cubeViews;

        public TextureArray2D(int numLayer, int numMipmaps, Size3 size, Format format, bool createUav)
        {
            Debug.Assert(size.Depth == 1);
            Size = size;
            NumMipmaps = numMipmaps;
            NumLayers = numLayer;
            this.Format = format;
           
            handle = new SharpDX.Direct3D11.Texture2D(Device.Get().Handle, CreateTextureDescription(createUav));

            CreateTextureViews(createUav);
        }

        public TextureArray2D(ImageLoader.Image image)
        {
            Size = image.GetSize(0);
            Debug.Assert(Size.Depth == 1);
            NumMipmaps = image.NumMipmaps;
            NumLayers = image.NumLayers;
            Format = image.Format.DxgiFormat;

            var data = new DataRectangle[NumLayers * NumMipmaps];
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
                {
                    var mip = image.Layers[curLayer].Mipmaps[curMipmap];
                    var idx = GetSubresourceIndex(curLayer, curMipmap);
                    data[idx].DataPointer = mip.Bytes;
                    // The distance (in bytes) from the beginning of one line of a texture to the next line.
                    data[idx].Pitch = (int)(mip.Size / mip.Height);
                }
            }

            handle = new Texture2D(Device.Get().Handle, CreateTextureDescription(false), data);

            CreateTextureViews(false);
        }

        public ShaderResourceView GetCubeView(int mipmap)
        {
            Debug.Assert(cubeViews != null);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(mipmap >= 0);
            return cubeViews[mipmap];
        }

        public override bool Is3D => false;

        protected override SharpDX.Direct3D11.Resource GetStagingTexture(int layer, int mipmap)
        {
            Debug.Assert(IO.SupportedFormats.Contains(Format));

            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(layer >= 0);
            Debug.Assert(layer < NumLayers);

            var newSize = Size.GetMip(mipmap);

            var desc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Format = Format,
                Height = newSize.Height,
                Width = newSize.Width,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Staging
            };

            // create staging texture
            var staging = new Texture2D(Device.Get().Handle, desc);

            // copy data to staging resource
            Device.Get().CopySubresource(handle, staging,
                GetSubresourceIndex(layer, mipmap), 0,
                newSize
            );

            return staging;
        }

        public override TextureArray2D CreateT(int numLayer, int numMipmaps, Size3 size, Format format, bool createUav)
        {
            return new TextureArray2D(numLayer, numMipmaps, size, format, createUav);
        }

        protected override SharpDX.Direct3D11.Resource GetHandle()
        {
            return handle;
        }

        public override void Dispose()
        {
            if (cubeViews != null)
            {
                foreach (var view in cubeViews)
                {
                    view?.Dispose();
                }
            }

            base.Dispose();
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

                    views[GetSubresourceIndex(curLayer, curMipmap)] = new ShaderResourceView(Device.Get().Handle, handle, desc);

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

                    rtViews[GetSubresourceIndex(curLayer, curMipmap)] = new RenderTargetView(Device.Get().Handle, handle, rtDesc);
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

        private Texture2DDescription CreateTextureDescription(bool createUav)
        {
            Debug.Assert(NumLayers > 0);
            Debug.Assert(NumMipmaps > 0);
            Debug.Assert(Size.Min > 0);
            Debug.Assert(Format != Format.Unknown);

            // render target required for mip map generation
            BindFlags flags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            if (createUav)
                flags |= BindFlags.UnorderedAccess;

            var optionFlags = ResourceOptionFlags.None;
            if (NumLayers == 6)
                optionFlags |= ResourceOptionFlags.TextureCube;

            return new Texture2DDescription
            {
                ArraySize = NumLayers,
                BindFlags = flags,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format,
                Height = Size.Height,
                MipLevels = NumMipmaps,
                OptionFlags = optionFlags,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = Size.Width
            };
        }
    }
}
