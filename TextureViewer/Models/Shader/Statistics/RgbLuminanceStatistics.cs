using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models.Shader.Statistics
{
    /// <summary>
    /// this shader converts all values into srgb space before in first iteration (if asked).
    /// the alpha component will be the luminance of the rgb vector.
    /// GetCombineFunction() must be overwritten again to deliver a return value
    /// </summary>
    public class RgbLuminanceStatistics : StatisticsShader
    {
        private readonly bool useSrgb;

        public RgbLuminanceStatistics(bool useSrgb)
        {
            this.useSrgb = useSrgb;

            init();
        }

        protected override string GetFunctions()
        {
            return @"vec4 toSrgb(vec4 a){
                        for(int i = 0; i < 3; ++i){
                            if( a[i] > 1.0 ) a[i] = 1.0;
                            else if( a[i] < 0.0 ) a[i] = 0.0;
                            else if( a[i] < 0.0031308 ) a[i] = 12.92 * a[i];
                            else a[i] = 1.055 * pow( a[i], 0.41666) - 0.055;
                        }
                        return a;
                    }";
        }

        protected override string GetSingleCombine()
        {
            // convert all values to srgb in the first iteration
            var res = "";
            if(useSrgb)
                // convert all values to srgb in the first iteration
                res += @"if( stride == 2 && direction == ivec2(1, 0) ){
                        a = toSrgb(a);}
                    ";

            // convert alpha to luminance instead
            res += @"a.a = dot(a.rgb, vec3(0.2126, 0.7152, 0.0722));
                    return a;";

            return res;
        }

        protected override string GetCombineFunction()
        {
            var res = "";
            if(useSrgb)
                // convert both to srgb
                res += @"if( stride == 2 && direction == ivec2(1, 0) ){
                        a = toSrgb(a);
                        b = toSrgb(b);
                    }
                    ";

            // convert alpha to luminance instead
            res += @"a.a = dot(a.rgb, vec3(0.2126, 0.7152, 0.0722));
                    b.a = dot(b.rgb, vec3(0.2126, 0.7152, 0.0722));";

            return res;
        }
    }
}
