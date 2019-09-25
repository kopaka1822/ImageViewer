using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;

namespace ImageFramework.Model.Shader
{
    public class QuadShader : IDisposable
    {
        public VertexShader Vertex => shader.Vertex;

        private readonly DirectX.Shader shader;

        public QuadShader()
        {
            shader = new DirectX.Shader(DirectX.Shader.Type.Vertex, GetSource(), "QuadShader");
        }

        private static string GetSource()
        {
            return @"
struct VertexOut
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

VertexOut main(uint id : SV_VertexID)
{
    VertexOut o;
    o.texcoord = float2((id << 1) & 2, id & 2);
    o.projPos = float4(o.texcoord * float2(2.0, -2.0) + float2(-1.0, 1.0), 0.0, 1.0);
    return o;
}           ";
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
