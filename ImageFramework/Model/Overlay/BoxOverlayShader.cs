using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using Microsoft.SqlServer.Server;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;
using Format = SharpDX.DXGI.Format;

namespace ImageFramework.Model.Overlay
{
    /// <summary>
    /// image overlay that draws colored boxes (zoom boxes)
    /// </summary>
    public class BoxOverlayShader : IDisposable
    {
        private readonly DirectX.Shader vertex;
        private readonly DirectX.Shader pixel;
        private readonly InputLayout input;

        public BoxOverlayShader()
        {
            vertex = new DirectX.Shader(DirectX.Shader.Type.Vertex, GetVertexSource(), "BoxOverlayVertex", out var bytecode);
            pixel = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetPixelSource(), "BoxOverlayPixel");

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
            public Color Color;
            public int Border;
        }

        public void Draw(ObservableCollection<BoxOverlay.Box> boxes, UploadBuffer upload, int mipmap, Size2 dim)
        {
            var dev = Device.Get();
            IntBox ibox = new IntBox();
            for (var i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];
                ibox.Color = box.Color;
                ibox.Border = Math.Max(box.Border >> mipmap, 1);
                // calc integer start and end
                ibox.Start = box.Start.ToPixels(dim);
                ibox.End = box.End.ToPixels(dim);

                upload.SetData(ibox);
                dev.Pixel.SetConstantBuffer(0, upload.Handle);
                dev.ContextHandle.DrawInstanced(4, 1, i * 4, 0);
            }
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
            return @"
cbuffer BoxBuffer : register(b0)
{
    int2 start;
    int2 end;
    float4 color;
    int border;
};

float4 main(float4 pos : SV_POSITION) : SV_TARGET
{
    int2 coord = int2(pos.xy);
    
    // minimal distance to any border
    int2 dist = min(coord - start, end - coord);
    int minDist = min(dist.x, dist.y);
    
    if(minDist >= border) discard;
    
    return color;
}
";
        }
    }
}
