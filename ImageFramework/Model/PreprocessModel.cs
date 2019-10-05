using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model
{
    /// <summary>
    /// this model is applied after the filters
    /// </summary>
    public class PreprocessModel : IDisposable
    {
        private readonly MinDefaultStatsticsShader minStatsShader = new MinDefaultStatsticsShader();
        private readonly MaxDefaultStatsticsShader maxStatsShader = new MaxDefaultStatsticsShader();
        private readonly AvgDefaultStatisticsShader avgStatsShader = new AvgDefaultStatisticsShader();

        internal StatisticsModel GetStatistics(TextureArray2D image, int layer, int mipmap, PixelValueShader pixelValueShader, TextureCache cache)
        {
            var stats = new StatisticsModel
            {
                Min = minStatsShader.Run(pixelValueShader, image, cache, layer, mipmap),
                Max = maxStatsShader.Run(pixelValueShader, image, cache, layer, mipmap),
                Avg = avgStatsShader.Run(pixelValueShader, image, cache, layer, mipmap)
            };
            return stats;
        }

        public void Dispose()
        {
            minStatsShader?.Dispose();
            maxStatsShader?.Dispose();
            avgStatsShader?.Dispose();
        }
    }
}
