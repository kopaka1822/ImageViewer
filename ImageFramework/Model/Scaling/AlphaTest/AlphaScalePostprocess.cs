using ImageFramework.DirectX;
using ImageFramework.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Model.Scaling.AlphaTest
{
    internal class AlphaScalePostprocess : PostprocessBase
    {
        private float threshold = 0.5f;

        public override void Run(ITexture uav, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            throw new NotImplementedException();
        }
    }
}
