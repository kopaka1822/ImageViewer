using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;

namespace ImageFramework.Utility
{
    /// <summary>
    /// layer and mipmap count information (layer/mipmap > 0)
    /// </summary>
    public struct LayerMipmapCount
    {
        private int layers;
        private int mipmaps;

        public int Layers
        {
            get => layers;
            set
            {
                Debug.Assert(value > 0);
                layers = value;
            }
        }

        public int Mipmaps
        {
            get => mipmaps;
            set
            {
                Debug.Assert(value > 0);
                mipmaps = value;
            }
        }

        public bool IsMipmapInside(int mipmap)
        {
            return mipmap >= 0 && mipmap < Mipmaps;
        }

        public LayerMipmapCount(int layers, int mipmaps)
        {
            Debug.Assert(layers > 0);
            Debug.Assert(mipmaps > 0);
            this.layers = layers;
            this.mipmaps = mipmaps;
        }

        public static readonly LayerMipmapCount One = new LayerMipmapCount(1, 1);

        // iterates of all layers and mipmaps
        public IEnumerable<LayerMipmapSlice> Range
        {
            get
            {
                Debug.Assert(Layers > 0);
                Debug.Assert(Mipmaps > 0);
                for (int mip = 0; mip < Mipmaps; ++mip)
                for (int layer = 0; layer < Layers; ++layer)
                {
                    yield return new LayerMipmapSlice(layer, mip);
                }
            }
        }

        // iterates over all layers and mipmaps within the given range
        public IEnumerable<LayerMipmapSlice> RangeOf(LayerMipmapRange range)
        {
            int maxMip = range.IsSingleMipmap ? range.FirstMipmap + 1 : Mipmaps;
            int maxLayer = range.IsSingleLayer ? range.FirstLayer + 1 : Layers;

            for (int mip = range.FirstMipmap; mip < maxMip; ++mip)
            for (int layer = range.FirstLayer; layer < maxLayer; ++layer)
            {
                yield return new LayerMipmapSlice(layer, mip);
            }
        }

        // iterates over all layers with the given mipmap level
        public IEnumerable<LayerMipmapSlice> LayersOfMipmap(int mipLevel)
        {
            Debug.Assert(IsMipmapInside(mipLevel));
            for (int layer = 0; layer < Layers; ++layer)
            {
                yield return new LayerMipmapSlice(layer, mipLevel);
            }
        }

        public bool Equals(LayerMipmapCount other)
        {
            return Layers == other.Layers && Mipmaps == other.Mipmaps;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LayerMipmapCount other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (layers * 397) ^ mipmaps;
            }
        }

        public static bool operator ==(LayerMipmapCount left, LayerMipmapCount right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayerMipmapCount left, LayerMipmapCount right)
        {
            return !(left == right);
        }
    }
}
