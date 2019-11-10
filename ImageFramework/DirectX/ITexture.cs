using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.DirectX
{
    public interface ITexture : IDisposable
    {
        int Width { get; }

        int Height { get; }
        int Depth { get; }
        int NumMipmaps { get; }
        bool HasMipmaps { get; }
        int NumLayers { get; }
        Format Format { get; }

        ShaderResourceView View { get; }

        ShaderResourceView GetSrView(int layer, int mipmap);

        RenderTargetView GetRtView(int layer, int mipmap);

        UnorderedAccessView GetUaView(int mipmap);

        int GetWidth(int mipmap);
        int GetHeight(int mimpap);
        int GetDepth(int mipmap);
        Color[] GetPixelColors(int layer, int mipmap);
        unsafe byte[] GetBytes(int layer, int mipmap, uint size);
        void CopyPixels(int layer, int mipmap, IntPtr destination, uint size);

        ITexture GenerateMipmapLevels(int levels);
        ITexture CloneWithoutMipmaps(int mipmap = 0);
        ITexture Create(int numLayer, int numMipmaps, int width, int height, int depth, Format format, bool createUav);

        ITexture Clone();
    }
}
