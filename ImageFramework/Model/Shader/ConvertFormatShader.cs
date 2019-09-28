using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace ImageFramework.Model.Shader
{
    public class ConvertFormatShader : IDisposable
    {
        public PixelShader Pixel => shader.Pixel;

        private DirectX.Shader shader;

        public ConvertFormatShader()
        {
            var dev = DirectX.Device.Get();
            shader = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(), "ConvertFormatShader");

            // check supported format capabilities
            foreach (var f in IO.SupportedFormats)
            {
                var sup = dev.CheckFormatSupport(f);

                if((sup & FormatSupport.Texture2D) == 0)
                    throw new Exception($"Texture2D support for {f} is required");
                // TODO this can be optional
                if((sup & FormatSupport.MipAutogen) == 0)
                    throw new Exception($"MipAutogen support for {f} is required");

                if((sup & FormatSupport.RenderTarget) == 0)
                    throw new Exception($"RenderTarget support for {f} is required");
            }
        }

        public static int InputSrvBinding => 0;

        public static string FromSrgbFunction()
        {
            return @"float4 fromSrgb(float4 c){
                        float3 r;
                        for(int i = 0; i < 3; ++i){
                            if(c[i] > 1.0) r[i] = 1.0;
                            else if(c[i] < 0.0) r[i] = 0.0;
                            else if(c[i] <= 0.04045) r[i] = c[i] / 12.92;
                            else r[i] = pow((c[i] + 0.055)/1.055, 2.4);
                        }
                        return float4(r, c.a);
                    }";
        }

        public static string ToSrgbFunction()
        {
            return @"float4 toSrgb(float4 c){
                        float3 r;
                        for(int i = 0; i < 3; ++i){
                            if( c[i] > 1.0) r[i] = 1.0;
                            else if( c[i] < 0.0) r[i] = 0.0;
                            else if( c[i] <= 0.0031308) r[i] = 12.92 * c[i];
                            else r[i] = 1.055 * pow(c[i], 0.41666) - 0.055;
                        }
                        return float4(r, c.a);
                    }";
        }

        private static string GetSource()
        {
            return @"
Texture2D<float4> in_tex : register(t0);

SamplerState spoint : register(s0);

float4 main(float2 coord : TEXCOORD) : SV_TARGET
{
    return in_tex.Sample(spoint, coord);
}
";
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
