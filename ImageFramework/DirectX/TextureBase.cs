using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace ImageFramework.DirectX
{
    public abstract class TextureBase<T> : IDisposable where T : TextureBase<T>
    {
        public int Width { get; protected set; }

        public int Height { get; protected set; }
        public int Depth { get; protected set; }
        public int NumMipmaps { get; protected set; }
        public bool HasMipmaps { get; protected set; }
        public int NumLayers { get; protected set; }
        public Format Format { get; protected set; }

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
            return rtViews[GetSubresourceIndex(layer, mipmap)];
        }

        public UnorderedAccessView GetUaView(int mipmap)
        {
            Debug.Assert(uaViews != null);
            Debug.Assert(mipmap < NumMipmaps);
            Debug.Assert(mipmap >= 0);
            return uaViews[mipmap];
        }

        public int GetWidth(int mipmap)
        {
            return Math.Max(1, Width >> mipmap);
        }

        public int GetHeight(int mimpap)
        {
            return Math.Max(1, Height >> mimpap);
        }

        public int GetDepth(int mipmap)
        {
            return Math.Max(1, Depth >> mipmap);
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
                return Device.Get().GetColorData(staging, Format, 0, GetWidth(mipmap), GetHeight(mipmap), GetDepth(mipmap));
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
                Device.Get().GetData(staging, Format, 0, GetWidth(mipmap), GetHeight(mipmap), GetDepth(mipmap), destination, size);
            }
        }

        /// <summary>
        /// generates new mipmaps
        /// </summary>
        public T GenerateMipmapLevels(int levels)
        {
            Debug.Assert(!HasMipmaps);
            var newTex = Create(NumLayers, levels, Width, Height, Depth, Format, uaViews != null);

            // copy all layers
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                // copy image data of first level
                Device.Get().CopySubresource(GetHandle(), newTex.GetHandle(), GetSubresourceIndex(curLayer, 0), newTex.GetSubresourceIndex(curLayer, 0), Width, Height, Depth);
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

        /// <summary>
        /// creates a new texture that has only one mipmap level
        /// </summary>
        /// <returns></returns>
        public T CloneWithoutMipmaps(int mipmap = 0)
        {
            var newTex = Create(NumLayers, 1, GetWidth(mipmap), GetHeight(mipmap), GetDepth(mipmap), Format,
                uaViews != null);

            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                // copy data of first level
                Device.Get().CopySubresource(GetHandle(), newTex.GetHandle(), GetSubresourceIndex(curLayer, mipmap), newTex.GetSubresourceIndex(curLayer, 0), Width, Height, Depth);
            }

            return newTex;
        }

        public T Clone()
        {
            var newTex = Create(NumLayers, NumMipmaps, Width, Height, Depth, Format, uaViews != null);

            Device.Get().CopyResource(GetHandle(), newTex.GetHandle());
            return newTex;
        }

        public abstract T Create(int numLayer, int numMipmaps, int width, int height, int depth, Format format,
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
