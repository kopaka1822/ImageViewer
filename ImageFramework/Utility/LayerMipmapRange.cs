using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;

namespace ImageFramework.Utility
{
    /// <summary>
    /// layer and mipmap range information (can be a single slice or multiple layers/mipmaps)
    /// </summary>
    public class LayerMipmapRange
    {
        protected int layer;
        protected int mipmap;

        public virtual int Layer
        {
            get => layer;
            set
            {
                Debug.Assert(value >= -1);
                layer = value;
            }
        }

        public virtual int Mipmap
        {
            get => mipmap;
            set
            {
                Debug.Assert(value >= -1);
                mipmap = value;
            }
        }

        // returns layer and asserts that it is non negative
        public int SingleLayer
        {
            get
            {
                Debug.Assert(IsSingleLayer);
                return Layer;
            }
        }

        // returns mipmap and asserts that it is non negative
        public int SingleMipmap
        {
            get
            {
                Debug.Assert(IsSingleMipmap);
                return Mipmap;
            }
        }

        public LayerMipmapSlice Single => new LayerMipmapSlice(layer, mipmap);

        public bool AllLayer => Layer < 0;
        public bool AllMipmaps => Mipmap < 0;
        public bool IsSingleLayer => Layer >= 0;
        public bool IsSingleMipmap => Mipmap >= 0;

        public int FirstLayer => Math.Max(Layer, 0);
        public int FirstMipmap => Math.Max(Mipmap, 0);

        public bool IsSingleMipmapIn(int nMipmaps)
        {
            return Mipmap >= 0 && Mipmap < nMipmaps;
        }
        
        public bool IsSingleLayerIn(int nLayer)
        {
            return Layer >= 0 && Layer < nLayer;
        }

        public bool IsIn(LayerMipmapCount lm)
        {
            return IsSingleMipmapIn(lm.Mipmaps) && IsSingleLayerIn(lm.Layers);
        }

        public LayerMipmapRange(int layer, int mipmap)
        {
            Debug.Assert(layer >= -1);
            Debug.Assert(mipmap >= -1);
            this.layer = layer;
            this.mipmap = mipmap;
        }

        public static readonly LayerMipmapRange All = new LayerMipmapRange(-1, -1);
        public static readonly LayerMipmapRange MostDetailed = new LayerMipmapRange(-1, 0);

        public LayerMipmapRange OffsetMipmap(int i)
        {
            return new LayerMipmapRange(Layer, SingleMipmap + i);
        }
    }
}
