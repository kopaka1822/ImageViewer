using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Shader
{
    public class CheckersShader : IDisposable
    {
        private readonly ImageFramework.DirectX.Shader vertex;
        private readonly ImageFramework.DirectX.Shader pixel;
        private readonly Vector3 themeColor;
        private readonly ModelsEx models;

        private struct BufferData
        {
            public Matrix Transform;
            public Vector3 ThemeColor;
            public int Type;
        }

        public CheckersShader(ModelsEx models)
        {
            this.models = models;
            vertex = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Vertex, GetVertexSource(), "CheckersVertexShader");
            pixel = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Pixel, GetPixelSource(), "CheckersPixelShader");
            themeColor = new Vector3(models.Window.ThemeColor.Red, models.Window.ThemeColor.Green, models.Window.ThemeColor.Blue);
        }

        // for unit testing
        public static void CompileShaders()
        {
            var s1 = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Vertex, GetVertexSource(), "CheckersVertexShader");
            var s2 = new ImageFramework.DirectX.Shader(ImageFramework.DirectX.Shader.Type.Pixel, GetPixelSource(), "CheckersPixelShader");
            s1.Dispose();
            s2.Dispose();
        }

        public void Run(Matrix transform)
        {
            var v = models.ViewData;
            v.Buffer.SetData(new BufferData
            {
                Transform = transform,
                ThemeColor = themeColor,
                Type = (int)models.Settings.AlphaBackground
            });

            var dev = Device.Get();
            dev.Vertex.Set(vertex.Vertex);
            dev.Pixel.Set(pixel.Pixel);

            dev.Vertex.SetConstantBuffer(0, v.Buffer.Handle);
            dev.Pixel.SetConstantBuffer(0, v.Buffer.Handle);

            dev.DrawQuad();
        }

        private static string GetVertexSource()
        {
            return $@"
struct VertexOut {{
    float4 projPos : SV_POSITION;
}};

cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float3 themeColor;
    int type;
}};

VertexOut main(uint id: SV_VertexID) {{
    VertexOut o;
    float2 texcoord = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    o.projPos = float4(texcoord * float2(2, -2) + float2(-1, 1), 0, 1);
    o.projPos = mul(transform, o.projPos);
    return o;
}};
";
        }

        private static string GetPixelSource()
        {
            return $@"
cbuffer InfoBuffer : register(b0) {{
    matrix transform;
    float3 themeColor;
    int type;
}};

float4 main(float4 pos : SV_POSITION) : SV_TARGET {{
    if(type == 0) return float4(0,0,0,1);
    if(type == 1) return float4(1,1,1,1);
    if(type == 3) return float4(themeColor, 1);  

    // checkers pattern
    uint2 pixel = int2(pos.xy);
    pixel /= uint2(10, 10);
    bool isDark = ((pixel.x & 1) == 0);
    if((pixel.y & 1) == 0) isDark = !isDark;
    float c = isDark ? 0.7 : 0.5;
    return float4(c, c, c, 1.0);
}};
";
        }

        public void Dispose()
        {
            vertex?.Dispose();
            pixel?.Dispose();
        }
    }
}
