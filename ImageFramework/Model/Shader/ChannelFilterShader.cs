using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;

namespace ImageFramework.Model.Shader
{
    internal class ChannelFilterShader : IDisposable
    {
        private TransformShader transformR;
        private TransformShader transformG;
        private TransformShader transformB;
        private TransformShader transformA;
        private TransformShader transformRGB;

        private TransformShader TransformR =>
            transformR ?? (transformR = new TransformShader("return float4(value.rrr, 1.0)", "float4"));
        private TransformShader TransformG =>
            transformG ?? (transformG = new TransformShader("return float4(value.ggg, 1.0)", "float4"));
        private TransformShader TransformB =>
            transformB ?? (transformB = new TransformShader("return float4(value.bbb, 1.0)", "float4"));
        private TransformShader TransformA =>
            transformA ?? (transformA = new TransformShader("return float4(value.aaa, 1.0)", "float4"));
        private TransformShader TransformRGB =>
            transformRGB ?? (transformRGB = new TransformShader("return float4(value.rgb, 1.0)", "float4"));

        public ChannelFilterShader()
        {
            
        }

        public void Convert(ITexture src, ImagePipeline.ChannelFilters filter, UploadBuffer upload)
        {
            foreach (var lm in src.LayerMipmap.Range)
            {
                switch (filter)
                {
                    case ImagePipeline.ChannelFilters.Red:
                        TransformR.Run(src, lm, upload);
                        break;
                    case ImagePipeline.ChannelFilters.Green:
                        TransformG.Run(src, lm, upload);
                        break;
                    case ImagePipeline.ChannelFilters.Blue:
                        TransformB.Run(src, lm, upload);
                        break;
                    case ImagePipeline.ChannelFilters.Alpha:
                        TransformA.Run(src, lm, upload);
                        break;
                    case ImagePipeline.ChannelFilters.RGB:
                        TransformRGB.Run(src, lm, upload);
                        break;
                    case ImagePipeline.ChannelFilters.RGBA:
                        Debug.Assert(false); // should not happen! (would be a simple copy)
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            }
        }

        public void Dispose()
        {
            transformR?.Dispose();
            transformG?.Dispose();
            transformB?.Dispose();
            transformA?.Dispose();
            transformRGB?.Dispose();
        }
    }
}
