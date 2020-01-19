using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling
{
    internal interface IDownscalingShader : IDisposable
    {
        /// <param name="src">src texture as input (most detailed mip will be used)</param>
        /// <param name="dst">dst texture (may be same as source)</param>
        /// <param name="dstMipmap">dst mipmap level</param>
        /// <param name="hasAlpha">indicates if the texture has an alpha channel</param>
        /// <param name="upload">used for constant buffer</param>
        /// <param name="cache">used for temporary texture. Must be the dimension of src texture</param>
        void Run(ITexture src, ITexture dst, int dstMipmap, bool hasAlpha, UploadBuffer upload, ITextureCache cache);
    }
}
