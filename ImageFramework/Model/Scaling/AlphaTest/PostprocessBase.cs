using ImageFramework.DirectX;
using ImageFramework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Scaling.AlphaTest
{
    internal abstract class PostprocessBase : IPostprocess
    {
        public abstract void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache);
        public virtual void Dispose()
        {
            
        }
    }
}
