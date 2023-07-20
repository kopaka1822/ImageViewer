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

float3 fromSrgb(float3 c){{
	return float3(fromSrgb(c.x), fromSrgb(c.y), fromSrgb(c.z));
}}

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

// color tables from: https://www.kennethmoreland.com/color-advice/
static float4 inferno[] = {{
    float4(0, 0, 0, 0.0),
    float4(40, 11, 84, 0.14),
    float4(101, 21, 110, 0.29),
    float4(159, 42, 99, 0.43),
    float4(212, 72, 66, 0.57),
    float4(245, 125, 21, 0.71),
    float4(250, 193, 39, 0.86),
    float4(252, 255, 164, 1.0),
}};

static float4 coolWarm[] = {{
    float4(59, 76, 192, 0.0),
    float4(99, 125, 213, 0.142857143),
    float4(149, 173, 227, 0.285714286),
    float4(209, 220, 238, 0.428571429),
	float4(242, 242, 242, 0.5),
    float4(236, 215, 203, 0.571428571),
    float4(222, 158, 134, 0.714285714),
    float4(203, 99, 79, 0.857142857),
    float4(180,4,38, 1.0),
}};

static float4 blackBody[] = {{
    float4(0, 0, 0, 0),
    float4(65, 23, 18, 0.142857143),
    float4(128, 31, 27, 0.285714286),
    float4(188, 51, 32, 0.428571429),
    float4(224, 101, 10, 0.571428571),
    float4(232, 161, 26, 0.714285714),
    float4(231, 218, 48, 0.857142857),
    float4(255, 255, 255, 1),
}};

#define COLOR_TABLE_FUNC(SIZE)                            \
float3 getColorFromTable##SIZE(float v, float4 table[SIZE]) {{   \
	float4 c1 = table[0]; float4 c2 = table[1];           \
	[unroll] for(int i = 1; i < SIZE; i++) {{              \
        c2 = table[i];                                    \
		if(v < c2.w) break;                               \
		c1 = c2;                                          \
    }}                                                     \
    float3 c = lerp(fromSrgb(c1.rgb/255.0), fromSrgb(c2.rgb/255.0), (v - c1.w) / max(c2.w - c1.w, 0.0001));	\
	return c;                                             \
}}

COLOR_TABLE_FUNC(8)
COLOR_TABLE_FUNC(9)

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
    case {(int)HeatmapModel.ColorStyle.Grayscale}:
        color.r = fromSrgb(v);
        color.g = color.r;
        color.b = color.r;  
        break;
    case {(int)HeatmapModel.ColorStyle.Inferno}:
        color.rgb = getColorFromTable8(v, inferno);
        break;
    case {(int)HeatmapModel.ColorStyle.CoolWarm}:
        color.rgb = getColorFromTable9(v, coolWarm);
        break;
    case {(int)HeatmapModel.ColorStyle.BlackBody}:
        color.rgb = getColorFromTable8(v, blackBody);
        break;
    }}
    
    return color;
}}
";
        }
    }
}
