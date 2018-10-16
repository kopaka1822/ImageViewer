using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models.Shader.Statistics
{
    /// <summary>
    /// this shader converts all values into srgb space before in first iteration.
    /// GetSingleCombine() and GetCombineFunction() must be overwritten again to deliver a return value
    /// </summary>
    public class PreSrgbStatistics : StatisticsShader
    {
        protected override string GetFunctions()
        {
            return @"vec4 toSrgb(vec4 a){
                        for(int i = 0; i < 3; ++i){
                            if( a[i] >= 1.0 ) a[i] = 1.0;
                            else if( a[i] <= 0.0 ) a[i] = 0.0;
                            else if( a[i] < 0.0031308 ) a[i] = 12.92 * a[i];
                            else a[i] = 1.055 * pow( a[i], 0.41666) - 0.055;
                        }
                    }";
        }

        protected override string GetSingleCombine()
        {
            // convert all values to srgb in the first iteration
            return @"if( stride == 2 && direction == ivec2(1, 0) )
                        a = toSrgb(a);
                    ";
        }

        protected override string GetCombineFunction()
        {
            return @"if( stride == 2 && direction == ivec2(1, 0) ){
                        a = toSrgb(a);
                        b = toSrgb(b);
                    }
                    ";
        }
    }
}
