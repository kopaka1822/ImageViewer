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
        private readonly DirectX.Shader shader;
        private readonly DirectX.Shader geo;

        public QuadShader()
        {
            shader = new DirectX.Shader(DirectX.Shader.Type.Vertex, GetSource(), "QuadShader");
            geo = new DirectX.Shader(DirectX.Shader.Type.Geometry, GetGeoSource(), "QuadGeoShader");
        }

        public void Bind(bool useGeometry)
        {
            DirectX.Device.Get().Vertex.Set(shader.Vertex);
            if(useGeometry)
                DirectX.Device.Get().Geometry.Set(geo.Geometry);
        }

        public void Unbind()
        {
            DirectX.Device.Get().Vertex.Set(null);
            DirectX.Device.Get().Geometry.Set(null);
        }

        private static string GetSource()
        {
            return @"
struct VertexOut
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

VertexOut main(uint vid : SV_VertexID)
{
    uint id = vid % 3;
    VertexOut o;
    // draws triangle that fills entire screen with [0, 1] inside screen
    o.texcoord = float2((id << 1) & 2, id & 2);
    o.projPos = float4(o.texcoord * float2(2.0, -2.0) + float2(-1.0, 1.0), 0.0, 1.0);
    return o;
}           ";
        }

        private static string GetGeoSource()
        {
            return @"
struct GeometryIn
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
};

struct GeometryOut
{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
    uint depth : SV_RenderTargetArrayIndex;
};

[maxvertexcount(3)]
void main(triangle GeometryIn i[3], uint id : SV_PrimitiveID, inout TriangleStream<GeometryOut> stream)
{
    GeometryOut o;
    o.depth = id;
    for(uint c = 0; c < 3; ++c) 
    {
        o.projPos = i[c].projPos;
        o.texcoord = i[c].texcoord;
        stream.Append(o);
    }
    stream.RestartStrip();
}
";
        }

        public void Dispose()
        {
            shader?.Dispose();
        }
    }
}
