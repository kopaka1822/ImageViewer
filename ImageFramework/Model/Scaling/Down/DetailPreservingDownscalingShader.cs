using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling.Down
{
    internal class DetailPreservingDownscalingShader : IDownscalingShader
    {
        private readonly IDownscalingShader boxScalingShader;
        private readonly FastGaussShader gaussShader;
        private readonly DetailPreservingShaderCore coreShader;
        public DetailPreservingDownscalingShader(IDownscalingShader boxScalingShader, bool veryDetailed, QuadShader quad)
        {
            this.boxScalingShader = boxScalingShader;
            gaussShader = new FastGaussShader(quad);
            coreShader = new DetailPreservingShaderCore(veryDetailed, quad);
        }

        public void Dispose()
        {
            gaussShader?.Dispose();
            coreShader?.Dispose();
        }

        public void Run(ITexture src, ITexture dst, int srcMipmap, int dstMipmap, bool hasAlpha, UploadBuffer upload, ITextureCache cache)
        {
            // first execute the box scaling shader
            boxScalingShader.Run(src, dst, srcMipmap, dstMipmap, hasAlpha, upload, cache);

            // run fast 3x3 gaussian shader
            var guidanceTex = cache.GetTexture();
            gaussShader.Run(dst, guidanceTex, dstMipmap, hasAlpha, upload);

            // perform filter with guidance texture
            coreShader.Run(src, guidanceTex, dst, srcMipmap, dstMipmap, hasAlpha, upload);

            cache.StoreTexture(guidanceTex);
        }

        // unit testing purposes
        internal void CompileShaders()
        {
            gaussShader.CompileShaders();
        }
    }
}
