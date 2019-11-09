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

        public Texture3D(int numMipmaps, int width, int height, int depth, Format format, bool createUav)
        {
            Width = width;
            Height = height;
            Depth = depth;
            NumLayers = 1;
            NumMipmaps = numMipmaps;
            Format = format;

            handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, CreateTextureDescription(createUav));

            CreateTextureViews(createUav);
        }

        public Texture3D(ImageLoader.Image image, int layer = 0)
        {
            Width = image.GetWidth(0);
            Height = image.GetHeight(0);
            Depth = image.GetDepth(0);
            NumLayers = 1;
            NumMipmaps = image.NumMipmaps;
            Format = image.Format.DxgiFormat;

            var data = new DataBox[NumMipmaps];

            for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
            {
                var mip = image.Layers[layer].Mipmaps[curMipmap];
                var idx = curMipmap;
                data[idx].DataPointer = mip.Bytes;
                data[idx].SlicePitch = (int)(mip.Size / mip.Depth);
                data[idx].RowPitch = data[idx].SlicePitch / mip.Height;
            }

            handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, CreateTextureDescription(false), data);
            CreateTextureViews(false);
        }

        public override Texture3D Create(int numLayer, int numMipmaps, int width, int height, int depth, Format format, bool createUav)
        {
            Debug.Assert(numLayer == 1);
            return new Texture3D(numMipmaps, width, height, depth, format, createUav);
        }

        protected override Resource GetHandle()
        {
            return handle;
        }

        public Color[] GetPixelColors(int mipmap)
        {
            return GetPixelColors(0, mipmap);
        }

        protected override Resource GetStagingTexture(int layer, int mipmap)
        {
            Debug.Assert(IO.SupportedFormats.Contains(Format));

            Debug.Assert(layer == 0);
            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);

            var desc = new Texture3DDescription
            {
                Width = GetWidth(mipmap),
                Height = GetHeight(mipmap),
                Depth = GetDepth(mipmap),
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
            Device.Get().CopySubresource(handle, staging, GetSubresourceIndex(layer, mipmap), 0, 
                GetWidth(mipmap), GetHeight(mipmap), GetDepth(mipmap));

            return staging;
        }

        private Texture3DDescription CreateTextureDescription(bool createUav)
        {
            Debug.Assert(NumLayers > 0);
            Debug.Assert(NumMipmaps > 0);
            Debug.Assert(Width > 0);
            Debug.Assert(Height > 0);
            Debug.Assert(Depth > 0);
            Debug.Assert(Format != Format.Unknown);

            // render target required for mip map generation
            BindFlags flags = BindFlags.ShaderResource | BindFlags.RenderTarget;
            if (createUav)
                flags |= BindFlags.UnorderedAccess;

            return new Texture3DDescription
            {
                Format = Format,
                Width = Width,
                Height = Height,
                Depth = Depth,
                BindFlags = flags,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = NumMipmaps,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                Usage = ResourceUsage.Default
            };
        }

        public ShaderResourceView GetSrView(int mipmap)
        {
            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            return views[mipmap];
        }

        private void CreateTextureViews(bool createUav)
        {
            Debug.Assert(handle != null);

            // default view
            View = new ShaderResourceView(Device.Get().Handle, handle, new ShaderResourceViewDescription
            {
                Dimension =  ShaderResourceViewDimension.Texture3D,
                Format = Format,
                Texture3D = new ShaderResourceViewDescription.Texture3DResource
                {
                    MipLevels = NumMipmaps,
                    MostDetailedMip = 0
                }
            });

            views = new ShaderResourceView[NumMipmaps];
            rtViews = new RenderTargetView[NumMipmaps];
            if(createUav)
                uaViews = new UnorderedAccessView[NumMipmaps];

            for (int curMip = 0; curMip < NumMipmaps; ++curMip)
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

                if(createUav)
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
