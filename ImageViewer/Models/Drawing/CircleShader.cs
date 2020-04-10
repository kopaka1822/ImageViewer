using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = ImageFramework.Utility.Color;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Models.Drawing
{
    public class CircleShader : IDisposable
    {
        private readonly Shader vertex;
        private readonly Shader pixel;
        private readonly UploadBuffer pointBuffer;
        private readonly InputLayout input;

        public CircleShader()
        {
            vertex = new Shader(Shader.Type.Vertex, GetVertexSource(), "CircleVertex", out var bytecode);
            pixel = new Shader(Shader.Type.Pixel, GetPixelSource(), "PixelVertex");
            pointBuffer = new UploadBuffer(sizeof(float) * 2 * 4, BindFlags.VertexBuffer);
            input = new InputLayout(Device.Get().Handle, bytecode, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0)
            });
        }

        public void Draw(Float2 center, Float2 radius, Color color, UploadBuffer cbuffer)
        {
            // create the vertex buffer
            var data = new Float2[4];
            data[0] = center - radius;
            data[1] = new Float2(center.X + radius.X, center.Y - radius.Y);
            data[2] = new Float2(center.X - radius.X, center.Y + radius.Y);
            data[3] = center + radius;
            pointBuffer.SetData(data);

            cbuffer.SetData(color);

            // bind shader and input layout
            var dev = Device.Get();
            dev.Vertex.Set(vertex.Vertex);
            dev.Pixel.Set(pixel.Pixel);
            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.InputAssembler.InputLayout = input;
            dev.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(pointBuffer.Handle, 2 * sizeof(float), 0));

            dev.ContextHandle.DrawInstanced(4, 1, 0, 0);

            // unbind
            dev.InputAssembler.InputLayout = null;
            dev.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
        }

        private static string GetVertexSource()
        {
            return @"
struct VertexOut {
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD; // circle coordinates between -1 and 1
};

VertexOut main(float2 pos : POSITION, uint id: SV_VertexID) {
    VertexOut o;
    float2 texcoord = float2(((id << 1) & 2) / 2, (id & 2) / 2);
    o.projPos = pos;
    o.texcoord = texcoord * 2 - 1;
    return o;
}
";
        }

        private static string GetPixelSource()
        {
            return @"
cbuffer InfoBuffer : register(b0){
    float4 color;
};

struct VertexIn {
    float4 projPos : SV_POSITION;
    float2 texcoord : TEXCOORD; // circle coordinates between -1 and 1
};

float4 main(VertexIn i) : SV_TARGET {
    if(length(i.texcoord) >= 1)
        discard;
    return color;
};
";
        }

        public void Dispose()
        {
            vertex?.Dispose();
            pixel?.Dispose();
            pointBuffer?.Dispose();
            input?.Dispose();
        }
    }
}
