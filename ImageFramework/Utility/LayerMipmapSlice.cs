using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Utility
{
    /// <summary>
    /// a single mipmap and a single layer
    /// </summary>
    public sealed class LayerMipmapSlice : LayerMipmapRange
    {
        public override int Layer
        {
            get => layer;
            set
            {
                Debug.Assert(value >= 0);
                layer = value;
            }
        }

        public override int Mipmap
        {
            get => mipmap;
            set
            {
                Debug.Assert(value >= 0);
                mipmap = value;
            }
        }

        public LayerMipmapSlice(int layer, int mipmap)
        : base(layer, mipmap)
        {
            Debug.Assert(layer >= 0);
            Debug.Assert(mipmap >= 0);
        }

        public static readonly LayerMipmapSlice Mip0 = new LayerMipmapSlice(0, 0);
        public static readonly LayerMipmapSlice Mip1 = new LayerMipmapSlice(0, 1);
        public static readonly LayerMipmapSlice Mip2 = new LayerMipmapSlice(0, 2);
        public static readonly LayerMipmapSlice Mip3 = new LayerMipmapSlice(0, 3);
        public static readonly LayerMipmapSlice Mip4 = new LayerMipmapSlice(0, 4);
        public static readonly LayerMipmapSlice Mip5 = new LayerMipmapSlice(0, 5);
        public static readonly LayerMipmapSlice Mip6 = new LayerMipmapSlice(0, 6);
        public static readonly LayerMipmapSlice Layer0 = new LayerMipmapSlice(0, 0);
        public static readonly LayerMipmapSlice Layer1 = new LayerMipmapSlice(1, 0);
        public static readonly LayerMipmapSlice Layer2 = new LayerMipmapSlice(2, 0);
        public static readonly LayerMipmapSlice Layer3 = new LayerMipmapSlice(3, 0);
        public static readonly LayerMipmapSlice Layer4 = new LayerMipmapSlice(4, 0);
        public static readonly LayerMipmapSlice Layer5 = new LayerMipmapSlice(5, 0);

        public LayerMipmapSlice AddMipmap(int i)
        {
            return new LayerMipmapSlice(Layer, Mipmap + i);
        }
    }
}
