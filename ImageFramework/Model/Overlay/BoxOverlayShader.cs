using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using Microsoft.SqlServer.Server;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;
using Format = SharpDX.DXGI.Format;

namespace ImageFramework.Model.Overlay
{
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

        public void Draw(ObservableCollection<BoxOverlay.Box> boxes, UploadBuffer upload)
        {
            var dev = Device.Get();
            for (var i = 0; i < boxes.Count; i++)
            {
                var box = boxes[i];
                upload.SetData(box);
                dev.Pixel.SetConstantBuffer(0, upload.Handle);
                dev.ContextHandle.DrawInstanced(4, 1, i * 4, 0);
            }
        }

        public void Unbind()
        {
            var dev = Device.Get();
            dev.InputAssembler.InputLayout = null;
            dev.InputAssembler.SetVertexBuffers(0, null);
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
    return pos;
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
    int2 dist = min(abs(coord - start), abs(end - coord));
    int minDist = min(dist.x, dist.y);
    
    if(minDist >= border) discard;
    
    return color;
}
";
        }
    }
}
