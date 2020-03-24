using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace ImageFramework.DirectX
{
    public class Texture3D : TextureBase<Texture3D>
    {
        private readonly SharpDX.Direct3D11.Texture3D handle;

        public Texture3D(int numMipmaps, Size3 size, Format format, bool createUav, bool createRt = true)
        {
            Size = size;
            LayerMipmap = new LayerMipmapCount(1, numMipmaps);
            Format = format;

            handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, CreateTextureDescription(createUav, createRt));

            CreateTextureViews(createUav, createRt);
        }

        public Texture3D(ImageLoader.Image image, int layer = 0)
        {
            Size = image.GetSize(0);
            LayerMipmap = image.LayerMipmap;
            Format = image.Format.DxgiFormat;

            var data = new DataBox[LayerMipmap.Mipmaps];

            for (int curMipmap = 0; curMipmap < LayerMipmap.Mipmaps; ++curMipmap)
            {
                var mip = image.Layers[layer].Mipmaps[curMipmap];
                var idx = curMipmap;
                data[idx].DataPointer = mip.Bytes;
                data[idx].SlicePitch = (int)(mip.Size / mip.Depth);
                data[idx].RowPitch = data[idx].SlicePitch / mip.Height;
            }

            handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, CreateTextureDescription(false,true), data);
            CreateTextureViews(false,true);
        }

        public override Texture3D CreateT(LayerMipmapCount lm, Size3 size, Format format, bool createUav)
        {
            Debug.Assert(lm.Layers == 1);
            return new Texture3D(lm.Mipmaps, size, format, createUav);
        }

        protected override Resource GetHandle()
        {
            return handle;
        }

        public Color[] GetPixelColors(int mipmap)
        {
            return GetPixelColors(new LayerMipmapSlice(0, mipmap));
        }

        public override bool Is3D => true;

        protected override Resource GetStagingTexture(LayerMipmapSlice lm)
        {
            Debug.Assert(IO.SupportedFormats.Contains(Format) || Format == Format.R8_UInt);

            Debug.Assert(lm.Layer == 0);
            Debug.Assert(lm.IsIn(LayerMipmap));

            var mipDim = Size.GetMip(lm.Mipmap);
            var desc = new Texture3DDescription
            {
                Width = mipDim.Width,
                Height = mipDim.Height,
                Depth = mipDim.Depth,
                Format = Format,
                MipLevels = 1,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Staging
            };

            // create staging texture
            var staging = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, desc);

            // copy data to staging texture
            Device.Get().CopySubresource(handle, staging, GetSubresourceIndex(lm), 0, mipDim);

            return staging;
        }

        private Texture3DDescription CreateTextureDescription(bool createUav, bool createRt)
        {
            Debug.Assert(Size.Min > 0);
            Debug.Assert(Format != Format.Unknown);

            // check resource limits
            if (Size.X > Device.MAX_TEXTURE_3D_DIMENSION || 
                Size.Y > Device.MAX_TEXTURE_3D_DIMENSION || 
                Size.Z > Device.MAX_TEXTURE_3D_DIMENSION)
                throw new Exception($"Texture3D Dimensions may not exceed {Device.MAX_TEXTURE_3D_DIMENSION}x{Device.MAX_TEXTURE_3D_DIMENSION}x{Device.MAX_TEXTURE_3D_DIMENSION} but were {Size.X}x{Size.Y}x{Size.Z}");

            // render target required for mip map generation
            BindFlags flags = BindFlags.ShaderResource;
            if (createUav)
                flags |= BindFlags.UnorderedAccess;
            if (createRt)
                flags |= BindFlags.RenderTarget;

            return new Texture3DDescription
            {
                Format = Format,
                Width = Size.Width,
                Height = Size.Height,
                Depth = Size.Depth,
                BindFlags = flags,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = LayerMipmap.Mipmaps,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Default
            };
        }

        public ShaderResourceView GetSrView(int mipmap)
        {
            Debug.Assert(LayerMipmap.IsMipmapInside(mipmap));
            return views[mipmap];
        }

        private void CreateTextureViews(bool createUav, bool createRt)
        {
            Debug.Assert(handle != null);

            // default view
            View = new ShaderResourceView(Device.Get().Handle, handle, new ShaderResourceViewDescription
            {
                Dimension = ShaderResourceViewDimension.Texture3D,
                Format = Format,
                Texture3D = new ShaderResourceViewDescription.Texture3DResource
                {
                    MipLevels = LayerMipmap.Mipmaps,
                    MostDetailedMip = 0
                }
            });

            views = new ShaderResourceView[LayerMipmap.Mipmaps];
            if (createRt)
                rtViews = new RenderTargetView[LayerMipmap.Mipmaps];
            if (createUav)
                uaViews = new UnorderedAccessView[LayerMipmap.Mipmaps];

            for (int curMip = 0; curMip < LayerMipmap.Mipmaps; ++curMip)
            {
                views[curMip] = new ShaderResourceView(Device.Get().Handle, handle, new ShaderResourceViewDescription
                {
                    Dimension = ShaderResourceViewDimension.Texture3D,
                    Format = Format,
                    Texture3D = new ShaderResourceViewDescription.Texture3DResource
                    {
                        MipLevels = 1,
                        MostDetailedMip = curMip
                    }
                });
                if (createRt)
                    rtViews[curMip] = new RenderTargetView(Device.Get().Handle, handle, new RenderTargetViewDescription
                    {
                        Dimension = RenderTargetViewDimension.Texture3D,
                        Format = Format,
                        Texture3D = new RenderTargetViewDescription.Texture3DResource
                        {
                            MipSlice = curMip,
                            FirstDepthSlice = 0,
                            DepthSliceCount = -1 // all slices
                        }
                    });

                if (createUav)
                    uaViews[curMip] = new UnorderedAccessView(Device.Get().Handle, handle, new UnorderedAccessViewDescription
                    {
                        Dimension = UnorderedAccessViewDimension.Texture3D,
                        Format = Format,
                        Texture3D = new UnorderedAccessViewDescription.Texture3DResource
                        {
                            FirstWSlice = 0,
                            MipSlice = curMip,
                            WSize = -1 // all slices
                        }
                    });
            }
        }
    }
}
