using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Models.Display.Overlays
{
    class HeatmapOverlayShader
    {
        private readonly Shader vertex;
        private readonly Shader pixel;
        private readonly InputLayout input;

        public HeatmapOverlayShader()
        {
            vertex = new Shader(Shader.Type.Vertex, GetVertexSource(), "HeatmapOVerlayVertex", out var bytecode);
            pixel = new Shader(Shader.Type.Pixel, GetPixelSource(), "HeatmapOverlayPixel");

            var dev = Device.Get();
            input = new InputLayout(dev.Handle, bytecode, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0)
            });
        }

        public void Bind(VertexBufferBinding vertexBuffer)
        {
            var dev = Device.Get();
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

        public void Draw(HeatmapModel data, UploadBuffer upload, int mipmap, Size2 dim)
        {
            var dev = Device.Get();
            var ibox = new IntBox();
            ibox.Start = data.Start.ToPixels(dim);
            ibox.End = data.End.ToPixels(dim);
            ibox.Border = data.Border;
            ibox.Style = (int)data.Style;

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

{Utility.FromSrgbFunction() /*the heatmap scale should appear to be linear for humans => the shader outputs linear colors that will be converted to srgb. to prevent this we need to aplly the reverse transformation (fromSrgb)*/}

float3 getColorBlackBlueGreenRed(float v)
{{
	if(v < 0.2)
		return lerp(float3(0.0, 0.0, 0.0), float3(0.0, 0.0, 1.0), v * 5.0);
	if(v < 0.4)
		return lerp(float3(0.0, 0.0, 1.0), float3(0.0, 1.0, 1.0), (v - 0.2) * 5.0);
	if(v < 0.6)
		return lerp(float3(0.0, 1.0, 1.0), float3(0.0, 1.0, 0.0), (v - 0.4) * 5.0);
	if(v < 0.8)
		return lerp(float3(0.0, 1.0, 0.0), float3(1.0, 1.0, 0.0), (v - 0.6) * 5.0);
	return lerp(float3(1.0, 1.0, 0.0), float3(1.0, 0.0, 0.0), (v - 0.8) * 5.0);
}}

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
    case {(int)HeatmapModel.ColorStyle.BlackRed}:
        color.r = fromSrgb(v);
        break;
    case {(int)HeatmapModel.ColorStyle.BlackBlueGreenRed}:
        color.rgb = getColorBlackBlueGreenRed(v);
        break;
    }}
    
    return color;
}}
";
        }
    }
}
