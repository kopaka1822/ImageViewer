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
        int NumMipmaps { get; }
        bool HasMipmaps { get; }
        bool Is3D { get; }
        int NumLayers { get; }
        Format Format { get; }

        ShaderResourceView View { get; }

        ShaderResourceView GetSrView(int layer, int mipmap);

        RenderTargetView GetRtView(int layer, int mipmap);

        UnorderedAccessView GetUaView(int mipmap);

        Color[] GetPixelColors(int layer, int mipmap);
        unsafe byte[] GetBytes(int layer, int mipmap, uint size);
        void CopyPixels(int layer, int mipmap, IntPtr destination, uint size);

        ITexture GenerateMipmapLevels(int levels);
        ITexture CloneWithoutMipmaps(int mipmap = 0);

        /// <summary>
        /// inplace mipmap regeneration based on the number of internal levels
        /// </summary>
        void RegenerateMipmapLevels();
        ITexture Create(int numLayer, int numMipmaps, Size3 size, Format format, bool createUav);

        ITexture Clone();
    }
}
