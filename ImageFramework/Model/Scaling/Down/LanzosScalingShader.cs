using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.SqlServer.Server;
using SharpDX.Direct3D11;
using SharpDX.Mathematics.Interop;
using Device = ImageFramework.DirectX.Device;
using Format = SharpDX.DXGI.Format;

namespace ImageFramework.Model.Scaling.Down
{
    internal class LanzosScalingShader : DownscalingShaderBase
    {
        // base function: sin(pi * x * stretch) * sin(pi * x), x in [-1, 1]
        // stretch values in [1, 3]

        // kaiser (alpha=4) ~ lanzos (alpha=1)
        // see: https://www.wolframalpha.com/input/?i=plot+I_0%284*sqrt%281-x%5E2%29%29%2FI_0%284%29%2C+sin%28PI*x%29%2F%28PI*x%29%2C+x%3D0+to+1

        // for: sin(pi * x)^2
        // approximation of the integral from -1 to x: 0.89734 + x - x^3 / 9 
        // integral is very similar to a linear function (box scaling shader)

        // for stretch = 3 (x will still be in [-1, 1]): sinc(pi * x * 3) * sinc(pi * x)
        // aprox. integral is given in s_values (128 values between -1 and 1)
        public unsafe LanzosScalingShader(QuadShader quad) : base(@"
return valueTex.SampleLevel(valueSampler, x * 0.5 + 0.5, 0).x; 
", 3, quad)
        {
            // pin s_values
            fixed (float* pValues = s_values)
            {
                var ptr = new IntPtr(pValues);
                valueTex = new Texture1D(Device.Get().Handle, new Texture1DDescription
                {
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R32_Float,
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Default,
                    Width = s_values.Length
                }, ptr);
            }
            valueTexView = new ShaderResourceView(Device.Get().Handle, valueTex);
            valueSampler = new SamplerState(Device.Get().Handle, new SamplerStateDescription
            {
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                Filter = SharpDX.Direct3D11.Filter.MinMagLinearMipPoint
            });

            AdditionalBindings = @"
Texture1D valueTex : register(t1);
SamplerState valueSampler : register(s0);
";
        }

        protected override void BindAdditionalResources(Device dev)
        {
            dev.Pixel.SetShaderResource(1, valueTexView);
            dev.Pixel.SetSampler(0, valueSampler);
        }

        protected override void UnbindAdditionalResources(Device dev)
        {
            dev.Pixel.SetShaderResource(1, null);
            dev.Pixel.SetSampler(0, null);
        }

        public override void Dispose()
        {
            base.Dispose();
            valueTex?.Dispose();
            valueTexView?.Dispose();
            valueSampler?.Dispose();
        }

        private Texture1D valueTex;
        private ShaderResourceView valueTexView;
        private SamplerState valueSampler;

        // data for the integral sinc(pi*x)*sinc(pi*x/3) in [-3, 3] <=> sinc(3*pi*x)*sinc(pi*x) in [-1, 1]
        // same as nvidia lanzos filter
        private static readonly float[] s_values = {0.0f, 1.329914858851254e-06f, 1.081889974714777e-05f, 3.695781135289055e-05f, 8.825135562370786e-05f, 0.0001728068689200272f, 0.0002979042664513352f, 0.0004695617466069898f, 0.0006921129279268844f, 0.0009678117190914312f, 0.001296481338969903f, 0.001675223485668954f, 0.002098202689009087f, 0.002556519375341623f, 0.003038183150028154f, 0.003528195301562044f, 0.00400874660926471f, 0.004459533266284882f, 0.00485819019755698f, 0.005180837356226842f, 0.005402730828451295f, 0.005499006877847585f, 0.005445503532141826f, 0.005219641069714201f, 0.004801339912237471f, 0.004173952072939781f, 0.003325180538344859f, 0.002247959850391013f, 0.0009412707642068894f, -0.0005891377762734614f, -0.002330144959174813f, -0.004260796420452976f, -0.00635200559167569f, -0.008566435759664414f, -0.01085857653588106f, -0.01317502391809933f, -0.01545496818747754f, -0.0176308886451347f, -0.01962944879012184f, -0.02137258011988358f, -0.02277873744372692f, -0.02376430358823804f, -0.02424511678548694f, -0.02413809000614353f, -0.02336288815358548f, -0.02184362647860309f, -0.01951055189455664f, -0.01630166813441243f, -0.0121642659338009f, -0.007056320661633029f, -0.0009477220383416402f, 0.0061786962591423f, 0.01432635527393397f, 0.02348438108098794f, 0.0336273246056257f, 0.04471516280406754f, 0.05669358666081531f, 0.06949457483485534f, 0.08303724511071504f, 0.09722896925372956f, 0.1119667306020042f, 0.1271386979144865f, 0.1426259837881499f, 0.1583045514950149f, 0.1740472304897649f, 0.1897257981966301f, 0.2052130840702933f, 0.2203850513827757f, 0.2351228127310503f, 0.2493145368740649f, 0.2628572071499246f, 0.2756581953239646f, 0.2876366191807124f, 0.2987244573791542f, 0.3088674009037918f, 0.3180254267108459f, 0.3261730857256376f, 0.3332995040231216f, 0.339408102646413f, 0.3445160479185808f, 0.3486534501191924f, 0.3518623338793366f, 0.354195408463383f, 0.3557146701383655f, 0.3564898719909235f, 0.3565968987702668f, 0.3561160855730179f, 0.3551305194285068f, 0.3537243621046634f, 0.3519812307749018f, 0.3499826706299146f, 0.3478067501722575f, 0.3455268059028793f, 0.343210358520661f, 0.3409182177444443f, 0.3387037875764556f, 0.3366125784052328f, 0.3346819269439548f, 0.3329409197610533f, 0.331410511220573f, 0.3301038221343889f, 0.3290266014464351f, 0.3281778299118401f, 0.3275504420725424f, 0.3271321409150657f, 0.326906278452638f, 0.3268527751069322f, 0.3269490511563286f, 0.3271709446285531f, 0.327493591787223f, 0.327892248718495f, 0.3283430353755152f, 0.3288235866832179f, 0.3293135988347519f, 0.3297952626094382f, 0.3302535792957708f, 0.330676558499111f, 0.33105530064581f, 0.3313839702656886f, 0.3316596690568531f, 0.3318822202381729f, 0.3320538777183286f, 0.3321789751158599f, 0.3322635306291563f, 0.3323148241734269f, 0.3323409630850328f, 0.3323504520699211f, 0.3323517819847798f };
    }
}
