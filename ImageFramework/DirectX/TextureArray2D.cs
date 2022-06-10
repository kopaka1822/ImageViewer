using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct2D1.Effects;
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

        internal Texture2D Handle => handle;

        public TextureArray2D(LayerMipmapCount lm, Size3 size, Format format, bool createUav, bool createRtv = true)
        {
            Debug.Assert(size.Depth == 1);
            Size = size;
            LayerMipmap = lm;
            this.Format = format;
           
            handle = new SharpDX.Direct3D11.Texture2D(Device.Get().Handle, CreateTextureDescription(createUav, createRtv));

            CreateTextureViews(createUav, createRtv);
        }

        public TextureArray2D(ImageData image)
        {
            Size = image.Size;
            Debug.Assert(Size.Depth == 1);
            LayerMipmap = image.LayerMipmap;
            Format = image.Format.DxgiFormat;

            var data = new DataRectangle[LayerMipmap.Layers * LayerMipmap.Mipmaps];
            foreach (var lm in LayerMipmap.Range)
            {
                var mip = image.GetMipmap(lm);
                var idx = GetSubresourceIndex(lm);
                data[idx].DataPointer = mip.Bytes;
                // The distance (in bytes) from the beginning of one line of a texture to the next line.
                data[idx].Pitch = (int)(mip.ByteSize / (uint)mip.Size.Height);
            }

            handle = new Texture2D(Device.Get().Handle, CreateTextureDescription(false, true), data);

            CreateTextureViews(false, true);
        }

        /// <summary>
        /// create a texture array 2d from the bitmap source.
        /// </summary>
        public TextureArray2D(System.Windows.Media.Imaging.BitmapSource source)
        {
            Size = new Size3(source.PixelWidth, source.PixelHeight);
            LayerMipmap = LayerMipmapCount.One;
            Format = Format.B8G8R8A8_UNorm_SRgb;

            var bitmap = new WriteableBitmap(source);
            bitmap.Lock();
            handle = new Texture2D(Device.Get().Handle, CreateTextureDescription(false, true),
                new DataRectangle(bitmap.BackBuffer, bitmap.BackBufferStride));
            bitmap.Unlock();

            CreateTextureViews(false, true);
        }

        /// <summary>
        /// creates a 2d texture from an array of colors
        /// </summary>
        /// <param name="colors">colors in x-major order</param>
        /// <param name="size">size in pixels</param>
        public TextureArray2D(Color[] colors, Size3 size, bool createUav = false, bool createRtv = true)
        {
            Size = size;
            Debug.Assert(Size.Depth == 1);
            Debug.Assert(Size.X * Size.Y == colors.Length);
            LayerMipmap = LayerMipmapCount.One;
            Format = Format.R32G32B32A32_Float;

            var colorsHandle = GCHandle.Alloc(colors, GCHandleType.Pinned);

            var data = new DataRectangle
            {
                DataPointer = colorsHandle.AddrOfPinnedObject(),
                Pitch = size.X * 4 * 4 // 4 floats with 4 byte each
            };
            
            handle = new Texture2D(Device.Get().Handle, CreateTextureDescription(false, true), data);

            colorsHandle.Free();

            CreateTextureViews(createUav, createRtv);
        }

        public ShaderResourceView GetCubeView(int mipmap)
        {
            Debug.Assert(cubeViews != null);
            Debug.Assert(LayerMipmap.IsMipmapInside(mipmap));
            return cubeViews[mipmap];
        }

        public override bool Is3D => false;

        protected override SharpDX.Direct3D11.Resource GetStagingTexture(LayerMipmapSlice lm)
        {
            Debug.Assert(IO.SupportedFormats.Contains(Format));

            var newSize = Size.GetMip(lm.Mipmap);

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
                GetSubresourceIndex(lm), 0,
                newSize
            );

            return staging;
        }

        public override TextureArray2D CreateT(LayerMipmapCount lm, Size3 size, Format format, bool createUav, bool createRtv)
        {
            return new TextureArray2D(lm, size, format, createUav, createRtv);
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

        private void CreateTextureViews(bool createUav, bool createRtv)
        {
            Debug.Assert(handle != null);

            // default View
            var defaultDesc = new ShaderResourceViewDescription
            {
                Dimension = ShaderResourceViewDimension.Texture2DArray,
                Format = Format,
                Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                {
                    ArraySize = LayerMipmap.Layers,
                    FirstArraySlice = 0,
                    MipLevels = LayerMipmap.Mipmaps,
                    MostDetailedMip = 0
                }
            };
            View = new ShaderResourceView(Device.Get().Handle, handle, defaultDesc);

            if (HasCubemap)
            {
                cubeViews = new ShaderResourceView[LayerMipmap.Mipmaps];
                for (int curMipmap = 0; curMipmap < LayerMipmap.Mipmaps; ++curMipmap)
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
            views = new ShaderResourceView[LayerMipmap.Layers * LayerMipmap.Mipmaps];
            if(createRtv)
                rtViews = new RenderTargetView[LayerMipmap.Layers * LayerMipmap.Mipmaps];

            foreach (var lm in LayerMipmap.Range)
            {
                var desc = new ShaderResourceViewDescription
                {
                    Dimension = ShaderResourceViewDimension.Texture2DArray,
                    Format = Format,
                    Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                    {
                        MipLevels = 1,
                        MostDetailedMip = lm.Mipmap,
                        ArraySize = 1,
                        FirstArraySlice = lm.Layer
                    }
                };

                views[GetSubresourceIndex(lm)] = new ShaderResourceView(Device.Get().Handle, handle, desc);

                if(createRtv)
                {
                    var rtDesc = new RenderTargetViewDescription
                    {
                        Dimension = RenderTargetViewDimension.Texture2DArray,
                        Format = Format,
                        Texture2DArray = new RenderTargetViewDescription.Texture2DArrayResource
                        {
                            ArraySize = 1,
                            FirstArraySlice = lm.Layer,
                            MipSlice = lm.Mipmap
                        }
                    };

                    rtViews[GetSubresourceIndex(lm)] = new RenderTargetView(Device.Get().Handle, handle, rtDesc);
                }
            }

            if (createUav)
            {
                uaViews = new UnorderedAccessView[LayerMipmap.Mipmaps];
                for (int curMipmap = 0; curMipmap < LayerMipmap.Mipmaps; ++curMipmap)
                {
                    var desc = new UnorderedAccessViewDescription
                    {
                        Dimension = UnorderedAccessViewDimension.Texture2DArray,
                        Format = Format,
                        Texture2DArray = new UnorderedAccessViewDescription.Texture2DArrayResource
                        {
                            ArraySize = LayerMipmap.Layers,
                            FirstArraySlice = 0,
                            MipSlice = curMipmap
                        }
                    };

                    uaViews[curMipmap] = new UnorderedAccessView(Device.Get().Handle, handle, desc);
                }
            }
        }

        private Texture2DDescription CreateTextureDescription(bool createUav, bool createRtv)
        {
            Debug.Assert(NumLayers > 0);
            Debug.Assert(NumMipmaps > 0);
            Debug.Assert(Size.Min > 0);
            Debug.Assert(Format != Format.Unknown);

            // check resource limits
            if(Size.X > Device.MAX_TEXTURE_2D_DIMENSION || Size.Y > Device.MAX_TEXTURE_2D_DIMENSION)
                throw new Exception($"Texture2D Dimensions may not exceed {Device.MAX_TEXTURE_2D_DIMENSION}x{Device.MAX_TEXTURE_2D_DIMENSION} but were {Size.X}x{Size.Y}");

            if (NumLayers > Device.MAX_TEXTURE_2D_ARRAY_DIMENSION)
                throw new Exception($"Number of layers may not exceed {Device.MAX_TEXTURE_2D_ARRAY_DIMENSION} but was {NumLayers}");

            // render target required for mip map generation
            BindFlags flags = BindFlags.ShaderResource;
            if (createRtv)
                flags |= BindFlags.RenderTarget;
            if (createUav)
                flags |= BindFlags.UnorderedAccess;

            var optionFlags = ResourceOptionFlags.None;
            if (HasCubemap)
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
