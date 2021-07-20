using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.Model.Overlay
{
    public class ArrowOverlayShader : IDisposable
    {
        private readonly DirectX.Shader vertex;
        private readonly DirectX.Shader pixel;
        private readonly InputLayout input;

        public ArrowOverlayShader()
        {
            vertex = new DirectX.Shader(DirectX.Shader.Type.Vertex, GetVertexSource(), "ArrowOverlayVertex", out var bytecode);
            pixel = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetPixelSource(), "ArrowOverlayPixel");

            var dev = DirectX.Device.Get();
            input = new InputLayout(dev.Handle, bytecode, new InputElement[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0),
            });
        }

        public void Bind(VertexBufferBinding vertexBuffer)
        {
            var dev = DirectX.Device.Get();
            dev.Vertex.Set(vertex.Vertex);
            dev.Pixel.Set(pixel.Pixel);
            dev.InputAssembler.InputLayout = input;
            dev.InputAssembler.SetVertexBuffers(0, vertexBuffer);
            dev.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        public void Draw(ObservableCollection<ArrowOverlay.Arrow> arrows, UploadBuffer upload, int mipmap, Size2 dim, int vertexCountPerInstance)
        {
            var dev = DirectX.Device.Get();
            for (var i = 0; i < arrows.Count; ++i)
            {
                upload.SetData(arrows[i].Color);
                dev.Pixel.SetConstantBuffer(0, upload.Handle);
                dev.ContextHandle.DrawInstanced(vertexCountPerInstance, 1, i * vertexCountPerInstance, 0);
            }
        }

        public void Unbind()
        {
            var dev = DirectX.Device.Get();
            dev.InputAssembler.InputLayout = null;
            dev.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(null, 0, 0));
            dev.InputAssembler.PrimitiveTopology = dev.DefaultTopology;
        }

        public void Dispose()
        {
            vertex?.Dispose();
            pixel?.Dispose();
            input?.Dispose();
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
cbuffer PixelBuffer : register(b0)
{
    float4 color;
};

float4 main() : SV_TARGET
{
    return color;
}
";
        }
    }
}
