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
        public int NumMipmaps { get; protected set; }
        public bool HasMipmaps => NumMipmaps > 1;
        public bool HasCubemap => NumLayers == 6 && Size.Width == Size.Height;
        public int NumLayers { get; protected set; }
        public Format Format { get; protected set; }

        public abstract bool Is3D { get; }

        protected UnorderedAccessView[] uaViews;
        protected RenderTargetView[] rtViews;
        protected ShaderResourceView[] views;

        public ShaderResourceView View { get; protected set; }

        public ShaderResourceView GetSrView(int layer, int mipmap)
        {
            return views[GetSubresourceIndex(layer, mipmap)];
        }

        public RenderTargetView GetRtView(int layer, int mipmap)
        {
            Debug.Assert(rtViews != null);
            return rtViews[GetSubresourceIndex(layer, mipmap)];
        }

        public UnorderedAccessView GetUaView(int mipmap)
        {
            Debug.Assert(uaViews != null);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(mipmap >= 0);
            return uaViews[mipmap];
        }

        protected int GetSubresourceIndex(int layer, int mipmap)
        {
            Debug.Assert(layer < NumLayers);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(layer >= 0);
            Debug.Assert(mipmap >= 0);

            return layer * NumMipmaps + mipmap;
        }

        protected abstract Resource GetStagingTexture(int layer, int mipmap);

        public Color[] GetPixelColors(int layer, int mipmap)
        {
            // create staging texture
            using (var staging = GetStagingTexture(layer, mipmap))
            {
                // obtain data from staging resource
                return Device.Get().GetColorData(staging, Format, 0, Size.GetMip(mipmap));
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
                Device.Get().GetData(staging, Format, 0, Size.GetMip(mipmap), destination, size);
            }
        }

        public ITexture GenerateMipmapLevels(int levels)
        {
            return GenerateMipmapLevelsT(levels);
        }

        public T GenerateMipmapLevelsT(int levels)
        {
            Debug.Assert(!HasMipmaps);
            var newTex = CreateT(NumLayers, levels, Size, Format, uaViews != null);

            // copy all layers
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                // copy image data of first level
                Device.Get().CopySubresource(GetHandle(), newTex.GetHandle(), GetSubresourceIndex(curLayer, 0), newTex.GetSubresourceIndex(curLayer, 0), Size);
            }

            Device.Get().GenerateMips(newTex.View);

            return newTex;
        }

        /// <summary>
        /// inplace mipmap regeneration based on the number of internal levels
        /// </summary>
        public void RegenerateMipmapLevels()
        {
            Device.Get().GenerateMips(View);
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
            var newTex = CreateT(NumLayers, 1, Size.GetMip(mipmap), Format,
                uaViews != null);

            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                // copy data of first level
                Device.Get().CopySubresource(GetHandle(), newTex.GetHandle(), GetSubresourceIndex(curLayer, mipmap), newTex.GetSubresourceIndex(curLayer, 0), Size.GetMip(mipmap));
            }

            return newTex;
        }

        public ITexture Clone()
        {
            return CloneT();
        }

        public T CloneT()
        {
            var newTex = CreateT(NumLayers, NumMipmaps, Size, Format, uaViews != null);

            Device.Get().CopyResource(GetHandle(), newTex.GetHandle());
            return newTex;
        }

        public ITexture Create(int numLayer, int numMipmaps, Size3 size, Format format, bool createUav)
        {
            return CreateT(numLayer, numMipmaps, size, format, createUav);
        }

        public abstract T CreateT(int numLayer, int numMipmaps, Size3 size, Format format,
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
