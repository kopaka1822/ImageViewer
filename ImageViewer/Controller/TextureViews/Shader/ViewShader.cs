using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageViewer.Models;
using SharpDX;
using Color = ImageFramework.Utility.Color;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public abstract class ViewShader : IDisposable
    {
        private ImageFramework.DirectX.Shader vertex;
        private ImageFramework.DirectX.Shader pixel;
        protected readonly ModelsEx models;

        public struct CommonBufferData
        {
            public Vector2 CropX;
            public Vector2 CropY;

            public Vector2 CropZ;
            public float Multiplier;
            public int UseAbs;

            public Vector4 NanColor;

            public int CropLayer; // layer => -1 don't crop any layer. layer > 0 => crop this layer / discard others
#pragma warning disable 169 // never used
            private int pad0;
            private int pad1;
            private int pad3;
#pragma warning restore 169
        }

        protected ViewShader(ModelsEx models, string vertex, string pixel, string debugName)
        {
            this.models = models;
            this.vertex = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Vertex, vertex, debugName + "VertexViewShader");
            this.pixel = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Pixel, pixel, debugName + "PixelViewShader");
        }

        protected CommonBufferData GetCommonData()
        {
            var res = new CommonBufferData
            {
                Multiplier = models.Display.Multiplier,
                UseAbs = models.Display.DisplayNegative?1:0,
                NanColor = ColorToVec(models.Settings.NaNColor),
                CropLayer = -1
            };

            // set cropping
            if (models.Export.UseCropping && (models.Display.IsExporting || models.Display.ShowCropRectangle))
            {
                res.CropLayer = models.Export.Layer;

                int mipmap = Math.Max(models.Export.Mipmap, 0);
                float cropMaxX = models.Images.GetWidth(mipmap);
                float cropMaxY = models.Images.GetHeight(mipmap);
                float cropMaxZ = models.Images.GetDepth(mipmap);

                res.CropX.X = models.Export.CropStart.X / cropMaxX;
                res.CropX.Y = (models.Export.CropEnd.X + 1) / cropMaxX;
                res.CropY.X = models.Export.CropStart.Y / cropMaxY;
                res.CropY.Y = (models.Export.CropEnd.Y + 1) / cropMaxY;
                res.CropZ.X = models.Export.CropStart.Z / cropMaxZ;
                res.CropZ.Y = (models.Export.CropEnd.Z + 1) / cropMaxZ;
            }
            else // no cropping
            {
                res.CropX.X = 0.0f;
                res.CropX.Y = 1.0f;
                res.CropY.X = 0.0f;
                res.CropY.Y = 1.0f;
                res.CropZ.X = 0.0f;
                res.CropZ.Y = 1.0f;
            }
            return res;
        }

        protected static string ApplyColorCrop(string texcoord, string layer, bool is3D)
        {
            return $@"
if((cropLayer != -1 && cropLayer != {layer}) // gray all out if on the wrong layer
|| {texcoord}.x < cropX.x || {texcoord}.x > cropX.y // otherwise, gray out based on crop rectangle
|| {texcoord}.y < cropY.x || {texcoord}.y > cropY.y
{(is3D? $"|| {texcoord}.y < cropZ.x || {texcoord}.y > cropZ.y" : "")})
    color.rgb = clamp(color.rgb, -1.0, 1.0) * 0.5;
";
        }

        protected static string CommonShaderBufferData()
        {
            return @"
float2 cropX;
float2 cropY;

float2 cropZ;
float multiplier;
bool useAbs;

float4 nancolor;

int cropLayer;
int pad0_;
int pad1_;
int pad2_;
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
