using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace ImageFramework.DirectX
{
    public abstract class TextureBase<T> : ITexture where T : TextureBase<T>
    {
        public Size3 Size { get; protected set; }
        public bool HasCubemap => LayerMipmap.Layers == 6 && Size.Width == Size.Height;

        public LayerMipmapCount LayerMipmap { get; protected set; }
        public int NumLayers => LayerMipmap.Layers;
        public int NumMipmaps => LayerMipmap.Mipmaps;

        public Format Format { get; protected set; }

        public abstract bool Is3D { get; }

        protected UnorderedAccessView[] uaViews;
        protected RenderTargetView[] rtViews;
        protected ShaderResourceView[] views;

        public ShaderResourceView View { get; protected set; }
        public bool HasUaViews => uaViews != null;
        public bool HasSrViews => views != null;
        public bool HasRtViews => rtViews != null;

        public ShaderResourceView GetSrView(LayerMipmapSlice lm)
        {
            return views[GetSubresourceIndex(lm)];
        }

        public RenderTargetView GetRtView(LayerMipmapSlice lm)
        {
            Debug.Assert(rtViews != null);
            return rtViews[GetSubresourceIndex(lm)];
        }

        public UnorderedAccessView GetUaView(int mipmap)
        {
            Debug.Assert(uaViews != null);
            Debug.Assert(LayerMipmap.IsMipmapInside(mipmap));
            return uaViews[mipmap];
        }

        protected int GetSubresourceIndex(LayerMipmapSlice lm)
        {
            Debug.Assert(lm.IsIn(LayerMipmap));

            return lm.Layer * LayerMipmap.Mipmaps + lm.Mipmap;
        }

        protected abstract Resource GetStagingTexture(LayerMipmapSlice lm);

        public Color[] GetPixelColors(LayerMipmapSlice lm)
        {
            // create staging texture
            using (var staging = GetStagingTexture(lm))
            {
                // obtain data from staging resource
                return Device.Get().GetColorData(staging, Format, 0, Size.GetMip(lm.Mipmap));
            }
        }

        public unsafe byte[] GetBytes(LayerMipmapSlice lm, uint size)
        {
            byte[] res = new byte[size];
            fixed (byte* ptr = res)
            {
                CopyPixels(lm, new IntPtr(ptr), size);
            }

            return res;
        }

        public unsafe byte[] GetBytes(uint pixelSize)
        {
            uint size = 0;
            for (int mip = 0; mip < LayerMipmap.Mipmaps; ++mip)
            {
                size += (uint) (LayerMipmap.Layers * Size.GetMip(mip).Product) * pixelSize;
            }

            byte[] res = new byte[size];
            uint offset = 0;
            fixed (byte* ptr = res)
            {
                foreach (var lm in LayerMipmap.Range)
                {
                    var mipSize = (uint)Size.GetMip(lm.Mipmap).Product * pixelSize;
                    CopyPixels(lm, new IntPtr(ptr + offset), mipSize);
                    offset += mipSize;
                }
            }

            Debug.Assert(offset == size);

            return res;
        }

        public void CopyPixels(LayerMipmapSlice lm, IntPtr destination, uint size)
        {
            // create staging texture
            using (var staging = GetStagingTexture(lm))
            {
                // obtain data from staging resource
                Device.Get().GetData(staging, Format, 0, Size.GetMip(lm.Mipmap), destination, size);
            }
        }

        public ITexture CloneWithMipmaps(int nLevels)
        {
            var newTex = CreateT(new LayerMipmapCount(LayerMipmap.Layers, nLevels), Size, Format, uaViews != null);

            // copy all layers
            for (int curLayer = 0; curLayer < LayerMipmap.Layers; ++curLayer)
            {
                // copy image data of first level
                Device.Get().CopySubresource(GetHandle(), newTex.GetHandle(), 
                    GetSubresourceIndex(new LayerMipmapSlice(curLayer, 0)), 
                    newTex.GetSubresourceIndex(new LayerMipmapSlice(curLayer, 0)), Size);
            }

            return newTex;
        }

        public ITexture CloneWithoutMipmaps(int mipmap = 0)
        {
            return CloneWithoutMipmapsT(mipmap);
        }

        /// <summary>
        /// creates a new texture that has only one mipmap level
        /// </summary>
        /// <returns></returns>
        public T CloneWithoutMipmapsT(int mipmap = 0)
        {
            var newTex = CreateT(new LayerMipmapCount(LayerMipmap.Layers, 1), Size.GetMip(mipmap), Format,
                uaViews != null);

            for (int curLayer = 0; curLayer < LayerMipmap.Layers; ++curLayer)
            {
                // copy data of first level
                Device.Get().CopySubresource(GetHandle(), newTex.GetHandle(), 
                    GetSubresourceIndex(new LayerMipmapSlice(curLayer, mipmap)), 
                    newTex.GetSubresourceIndex(new LayerMipmapSlice(curLayer, 0)), Size.GetMip(mipmap));
            }

            return newTex;
        }

        public ITexture Clone()
        {
            return CloneT();
        }

        public bool HasSameDimensions(ITexture other)
        {
            if (LayerMipmap != other.LayerMipmap) return false;
            if (Size != other.Size) return false;
            if (GetType() != other.GetType()) return false;
            return true;
        }

        public T CloneT()
        {
            var newTex = CreateT(LayerMipmap, Size, Format, uaViews != null);

            Device.Get().CopyResource(GetHandle(), newTex.GetHandle());
            return newTex;
        }

        public ITexture Create(LayerMipmapCount lm, Size3 size, Format format, bool createUav)
        {
            return CreateT(lm, size, format, createUav);
        }

        public abstract T CreateT(LayerMipmapCount lm, Size3 size, Format format,
            bool createUav);

        protected abstract Resource GetHandle();

        public virtual void Dispose()
        {
            GetHandle()?.Dispose();
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

            if (rtViews != null)
            {
                foreach (var renderTargetView in rtViews)
                {
                    renderTargetView?.Dispose();
                }
            }
            View?.Dispose();
        }
    }
}
