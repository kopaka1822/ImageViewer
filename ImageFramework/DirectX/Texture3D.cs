using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.DirectX
{
    public class Texture3D : TextureBase, IDisposable
    {
        private readonly SharpDX.Direct3D11.Texture3D handle;
        private ShaderResourceView[] views;
        private UnorderedAccessView[] uaViews;

        public Texture3D(int numMipmaps, int width, int height, int depth, Format format)
        {
            Width = width;
            Height = height;
            Depth = depth;
            NumLayers = 1;
            NumMipmaps = numMipmaps;
            Format = format;

            handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, CreateTextureDescription());

            CreateTextureViews();
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

            handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, CreateTextureDescription(), data);
            CreateTextureViews();
        }

        public Texture3D Clone()
        {
            var newTex = new Texture3D(NumMipmaps, Width, Height, Depth, Format);

            Device.Get().CopyResource(handle, newTex.handle);
            return newTex;
        }

        public override Color[] GetPixelColors(int layer, int mipmap)
        {
            Debug.Assert(layer == 0);
            return GetPixelColors(mipmap);
        }

        public Color[] GetPixelColors(int mipmap)
        {
            throw new NotImplementedException();
        }

        private Texture3DDescription CreateTextureDescription()
        {
            Debug.Assert(NumLayers > 0);
            Debug.Assert(NumMipmaps > 0);
            Debug.Assert(Width > 0);
            Debug.Assert(Height > 0);
            Debug.Assert(Depth > 0);
            Debug.Assert(Format != Format.Unknown);

            return new Texture3DDescription
            {
                Format = Format,
                Width = Width,
                Height = Height,
                Depth = Depth,
                BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess | BindFlags.RenderTarget,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = NumMipmaps,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                Usage = ResourceUsage.Default
            };
        }

        public override UnorderedAccessView GetUaView(int mipmap)
        {
            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            return uaViews[mipmap];
        }

        public ShaderResourceView GetSrView(int mipmap)
        {
            Debug.Assert(mipmap >= 0);
            Debug.Assert(mipmap < NumMipmaps);
            return views[mipmap];
        }

        private void CreateTextureViews()
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

                uaViews[curMip] = new UnorderedAccessView(Device.Get().Handle, handle, new UnorderedAccessViewDescription
                {
                    Dimension = UnorderedAccessViewDimension.Texture3D,
                    Format = Format,
                    Texture3D = new UnorderedAccessViewDescription.Texture3DResource
                    {
                        FirstWSlice = 0,
                        MipSlice = curMip,
                        WSize = Depth
                    }
                });
            }
        }

        public void Dispose()
        {
            handle?.Dispose();
            if (views != null)
            {
                foreach (var shaderResourceView in views)
                {
                    shaderResourceView?.Dispose();
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
        }
    }
}
