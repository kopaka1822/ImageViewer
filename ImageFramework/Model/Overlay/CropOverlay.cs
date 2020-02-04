using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Overlay
{
    /// <summary>
    /// image overlay that grays out an area
    /// </summary>
    public class CropOverlay : OverlayBase
    {
        private readonly QuadShader quad;
        private readonly UploadBuffer cbuffer;
        private readonly ImagesModel images;
        private DirectX.Shader shader;
        private DirectX.Shader shader3D;

        private DirectX.Shader Shader => shader ?? (shader = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                             GetPixelSource(ShaderBuilder.Builder2D), "CropOverlayPixel"));
        private DirectX.Shader Shader3D => shader3D ?? (shader3D = new DirectX.Shader(DirectX.Shader.Type.Pixel,
                                             GetPixelSource(ShaderBuilder.Builder3D), "CropOverlay3DPixel"));
        public CropOverlay(Models models)
        {
            quad = models.SharedModel.QuadShader;
            cbuffer = models.SharedModel.Upload;
            images = models.Images;
        }

        // start of the crop area (mipmap level 0)
        public Size3? Start { get; set; }

        // end of the crop area (mipmap level 0)
        public Size3? End { get; set; }

        public int Layer { get; set; } = -1;

        private struct BufferData
        {
            public float StartX;
            public float StartY;
            public float StartZ;
            public int CropLayer;
            public float EndX;
            public float EndY;
            public float EndZ;
            public float Depth;
            public int Layer;
        }

        public override void Render(int layer, int mipmap, Size3 size)
        {
            var data = new BufferData
            {
                CropLayer = Layer,
                Layer = layer,
                Depth = size.Depth,
            };

            // set crop boundaries
            if (!Start.HasValue || !End.HasValue)
            {
                // draw entire screen black
                data.CropLayer = -2;
            }
            else
            {
                // keep the spot between start and end bright
                data.StartX = Start.Value.X;
                data.StartY = Start.Value.Y;
                data.StartZ = Start.Value.Z;
                data.EndX = End.Value.X;
                data.EndY = End.Value.Y;
                data.EndZ = End.Value.Z;
            }

            cbuffer.SetData(data);
            var dev = Device.Get();
            quad.Bind(images.Is3D);
            dev.Pixel.Set(images.Is3D ? Shader3D.Pixel : Shader.Pixel);

            dev.DrawFullscreenTriangle(size.Depth);

            quad.Unbind();
            dev.Pixel.Set(null);
        }

        public override void Dispose()
        {
            shader?.Dispose();
            shader3D?.Dispose();
        }

        private string GetPixelSource(IShaderBuilder builder)
        {
            return $@"
cbuffer CropBuffer : register(b0)
{{
    float3 start;
    int cropLayer; // layer for cropping (can be -1)
    float3 end;
    float depth;
    int layer; // current layer
}};

struct PixelIn
{{
    float2 texcoord : TEXCOORD;
    float4 projPos : SV_POSITION;
#if {builder.Is3DInt}
    uint depth : SV_RenderTargetArrayIndex;
#endif
}};

float4 main(PixelIn i) : SV_TARGET
{{
    float3 coord = float3(i.projPos, 0.5);
#if {builder.Is3DInt}
    coord.z = i.depth / depth;
#endif
    
	if((cropLayer != -1 && cropLayer != layer) // gray all out if on the wrong layer
    || i.texcoord.x < start.x || i.texcoord.x > end.x // otherwise, gray out based on crop rectangle
    || i.texcoord.y < start.y || i.texcoord.y > end.y
    || i.texcoord.z < start.z || i.texcoord.z > end.z)
        return float4(0.0, 0.0, 0.0, 0.5); // gray out this area
    
    discard;
}}
";
        }
    }
}
