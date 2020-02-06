using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;
using Color = ImageFramework.Utility.Color;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public abstract class ViewShader : IDisposable
    {
        private ImageFramework.DirectX.Shader vertex;
        private ImageFramework.DirectX.Shader pixel;
        protected readonly ModelsEx models;

        public struct CommonBufferData
        {
            public float Multiplier;
            public int UseAbs;
            public int UseOverlay;
#pragma warning disable 169 // never used
            private int pad0;
#pragma warning restore 169

            public Vector4 NanColor;
        }

        protected ViewShader(ModelsEx models, string vertex, string pixel, string debugName)
        {
            this.models = models;
            this.vertex = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Vertex, vertex, debugName + "VertexViewShader");
            this.pixel = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Pixel, pixel, debugName + "PixelViewShader");
        }

        protected CommonBufferData GetCommonData(ShaderResourceView overlay)
        {
            var res = new CommonBufferData
            {
                Multiplier = models.Display.Multiplier,
                UseAbs = models.Display.DisplayNegative?1:0,
                NanColor = ColorToVec(models.Settings.NaNColor),
                UseOverlay = overlay == null ? 0 : 1
            };

            return res;
        }

        protected static string ApplyOverlay2D(string texcoord, string color)
        {
            return $@"
if(useOverlay) {{
    float4 ol = overlay.Sample(texSampler, {texcoord});
    {CalcOverlay(color)}
}}
";
        }

        protected static string ApplyOverlay3D(string texcoord, string color)
        {
            return $@"
if(useOverlay) {{
    float4 ol = overlay.Sample(texSampler, {texcoord});
    {CalcOverlay(color)}
}}
";
        }

        protected static string ApplyOverlayCube(string dir, string color)
        {
            return $@"
if(useOverlay) {{
    float4 ol = overlay.Sample(texSampler, {dir});
    {CalcOverlay(color)}
}}
";
        }

        private static string CalcOverlay(string color)
        {
            // overlay has alpha premultiplied color and alpha channel contains inverse alpha (occlusion)
            return $@"
{color}.rgb = ol.rgb + ol.a * {color}.rgb;
{color}.a = 1.0 - (ol.a * (1.0 - {color}.a));
";
        }

        protected static string CommonShaderBufferData()
        {
            return @"
float multiplier;
bool useAbs;
bool useOverlay;
int pad0_;

float4 nancolor;
";
        }

        /// <summary>
        /// adds srgb conversion and conditional abs
        /// </summary>
        /// <returns></returns>
        protected static string ApplyColorTransform()
        {
            return @"
if(useAbs) color.rgb = abs(color.rgb);
if(any(isnan(color))) color = nancolor;
else color = toSrgb(color);
";
        }

        protected void BindShader(Device dev)
        {
            dev.Vertex.Set(vertex.Vertex);
            dev.Pixel.Set(pixel.Pixel);
        }

        protected void UnbindShader(Device dev)
        {
            dev.Vertex.Set(null);
            dev.Pixel.Set(null);
        }

        public virtual void Dispose()
        {
            vertex?.Dispose();
            pixel?.Dispose();
        }

        private Vector4 ColorToVec(Color c)
        {
            return new Vector4(c.Red, c.Green, c.Blue, c.Alpha);
        }
    }
}
