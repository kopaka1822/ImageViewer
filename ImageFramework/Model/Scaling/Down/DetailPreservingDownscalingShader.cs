using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling.Down
{
    internal class DetailPreservingDownscalingShader : IDownscalingShader
    {
        private readonly IDownscalingShader boxScalingShader;
        private readonly FastGaussShader gaussShader;
        private readonly DetailPreservingShaderCore coreShader;
        public DetailPreservingDownscalingShader(IDownscalingShader boxScalingShader, bool veryDetailed)
        {
            this.boxScalingShader = boxScalingShader;
            gaussShader = new FastGaussShader();
            coreShader = new DetailPreservingShaderCore(veryDetailed);
        }

        public void Dispose()
        {
            gaussShader?.Dispose();
            coreShader?.Dispose();
        }

        public void Run(ITexture src, ITexture dst, int dstMipmap, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            // first execute the box scaling shader
            boxScalingShader.Run(src, dst, dstMipmap, hasAlpha, upload, cache);

            // run fast 3x3 gaussian shader
            var guidanceTex = cache.GetTexture();
            gaussShader.Run(src, guidanceTex, dstMipmap, hasAlpha, upload);

            // perform filter with guidance texture
            coreShader.Run(src, guidanceTex, dst, dstMipmap, hasAlpha, upload);

            cache.StoreTexture(guidanceTex);
        }

        // unit testing purposes
        internal void CompileShaders()
        {
            gaussShader.CompileShaders();
        }
    }
}
