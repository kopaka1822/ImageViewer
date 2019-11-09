using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.DirectX
{
    public abstract class TextureBase
    {
        public int Width { get; protected set; }

        public int Height { get; protected set; }
        public int Depth { get; protected set; }
        public int NumMipmaps { get; protected set; }
        public bool HasMipmaps { get; protected set; }
        public int NumLayers { get; protected set; }
        public Format Format { get; protected set; }

        public ShaderResourceView View { get; protected set; }

        public abstract UnorderedAccessView GetUaView(int mipmap);

        public abstract Color[] GetPixelColors(int layer, int mipmap);

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
    }
}
