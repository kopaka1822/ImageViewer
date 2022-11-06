using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.DirectX
{
    public interface ITexture : IDisposable
    {
        Size3 Size { get; }
        bool HasCubemap { get; }
        bool Is3D { get; }
        LayerMipmapCount LayerMipmap { get; }
        int NumLayers { get; }
        int NumMipmaps { get; }
        Format Format { get; }

        ShaderResourceView View { get; }
        bool HasUaViews { get; }
        bool HasSrViews { get; }
        bool HasRtViews { get; }

        ShaderResourceView GetSrView(LayerMipmapSlice lm);

        RenderTargetView GetRtView(LayerMipmapSlice lm);

        UnorderedAccessView GetUaView(int mipmap);

        Color[] GetPixelColors(LayerMipmapSlice lm);

        byte[] GetPixelAlphas(LayerMipmapSlice lm);

        // gets bytes from the specified slice
        unsafe byte[] GetBytes(LayerMipmapSlice lm, uint size);

        // gets all bytes
        unsafe byte[] GetBytes(uint pixelSize);

        void CopyPixels(LayerMipmapSlice lm, IntPtr destination, uint size);

        /// <summary>
        /// Creates new texture with nLevels levels and clones the most detailed mipmap
        /// </summary>
        ITexture CloneWithMipmaps(int nLevels);

        /// <summary>
        /// Creates new texture with a single level with the size and data of the specified mipmap
        /// </summary>
        ITexture CloneWithoutMipmaps(int mipmap = 0);

        ITexture Create(LayerMipmapCount lm, Size3 size, Format format, bool createUav, bool createRtv = true);

        ITexture Clone();

        // indicates if both textures have the same dimension, layer, etc. (format excluded)
        bool HasSameDimensions(ITexture other);
    }
}
