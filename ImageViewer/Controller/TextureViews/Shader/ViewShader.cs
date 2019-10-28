using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public abstract class ViewShader : IDisposable
    {
        private ImageFramework.DirectX.Shader vertex;
        private ImageFramework.DirectX.Shader pixel;

        protected ViewShader(string vertex, string pixel, string debugName)
        {
            this.vertex = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Vertex, vertex, debugName + "VertexViewShader");
            this.pixel = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Pixel, pixel, debugName + "PixelViewShader");
        }

        protected static string ApplyColorCrop(string texcoord)
        {
            return $"if({texcoord}.x < crop.x || {texcoord}.x > crop.y || {texcoord}.y < crop.z || {texcoord}.y > crop.w)\n" +
                   "color.rgb = min(color.rgb, float3(1.0, 1.0, 1.0)) * float3(0.5, 0.5, 0.5);\n";
        }

        /// <summary>
        /// adds srgb conversion and conditional abs
        /// </summary>
        /// <returns></returns>
        protected static string ApplyColorTransform()
        {
            return @"
if(useAbs) color.rgb = abs(color.rgb);
color = toSrgb(color);
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

        public void Dispose()
        {
            vertex?.Dispose();
            pixel?.Dispose();
        }
    }
}
