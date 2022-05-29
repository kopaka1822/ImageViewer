using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model.Overlay
{
    class HeatmapOverlayShader
    {
        private readonly DirectX.Shader vertex;
        private readonly DirectX.Shader pixel;
        private readonly InputLayout input;

        public HeatmapOverlayShader()
        {
            vertex = new DirectX.Shader(DirectX.Shader.Type.Vertex, GetVertexSource(), "HeatmapOVerlayVertex", out var bytecode);
            pixel = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetPixelSource(), "HeatmapOverlayPixel");

            var dev = DirectX.Device.Get();
            input = new InputLayout(dev.Handle, bytecode, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0)
            });
        }

        public void Bind(VertexBufferBinding vertexBuffer)
        {
            var dev = DirectX.Device.Get();
            dev.Vertex.Set(vertex.Vertex);
            dev.Pixel.Set(pixel.Pixel);
            dev.InputAssembler.InputLayout = input;
            dev.InputAssembler.SetVertexBuffers(0, vertexBuffer);
        }

        private struct IntBox
        {
            public Size2 Start;
            public Size2 End;
            public int Border;
            public int Style;
        }

        public void Draw(HeatmapOverlay.Heatmap data, UploadBuffer upload, int mipmap, Size2 dim)
        {
            var dev = DirectX.Device.Get();
            var ibox = new IntBox();
            ibox.Start = data.Start.ToPixels(dim);
            ibox.End = data.End.ToPixels(dim);
            ibox.Border = data.Border;
            ibox.Style = (int) data.Style;

            upload.SetData(ibox);
            dev.Pixel.SetConstantBuffer(0, upload.Handle);
            dev.ContextHandle.Draw(4, 0);
        }

        public void Unbind()
        {
            var dev = Device.Get();
            dev.InputAssembler.InputLayout = null;
            dev.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
        }

        public void Dispose()
        {
            vertex.Dispose();
            pixel.Dispose();
            input.Dispose();
        }

        private string GetVertexSource()
        {
            return @"
float4 main(float2 pos : POSITION) : SV_POSITION
{
    return float4(pos.x, -pos.y, 0.0, 1.0);
}
";
        }

        private string GetPixelSource()
        {
            return $@"
cbuffer BoxBuffer : register(b0)
{{
    int2 start;
    int2 end;
    int border;
    int style;
}};

float4 main(float4 pos : SV_POSITION) : SV_TARGET
{{
    int2 coord = int2(pos.xy);
    
    // minimal distance to any border
    int2 dist = min(coord - start, end - coord);
    int minDist = min(dist.x, dist.y);
    
    if(minDist < border) return float4(0.0, 0.0, 0.0, 1.0); // border color

    // determine fill color
    float yStart = start.y + (float)border;
    float yEnd = end.y - (float)border;
    // percentage value
    float v = ((float)coord.y - yStart) / max(yEnd - yStart, 0.5);
    v = 1.0 - v;

    float4 color = float4(0.0, 0.0, 0.0, 1.0);
    switch(style)
    {{
    case {(int)HeatmapOverlay.Style.BlackRed}:
        color.r = v;
        break;
    case {(int)HeatmapOverlay.Style.BlackBlueGreenRed}:

        break;
    }}
    
    return color;
}}
";
        }
    }
}
