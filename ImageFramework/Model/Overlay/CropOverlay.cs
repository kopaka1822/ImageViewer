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

        private bool isEnabled = true;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if(value == isEnabled) return;
                isEnabled = value;
                OnHasChanged();
            }
        }

        private Float3? start;
        // start of the crop area (mipmap level 0)
        public Float3? Start
        {
            get => start;
            set
            {
                if (value == start) return;
                start = value;
                if(isEnabled)
                    OnHasChanged();
            }
        }

        private Float3? end;
        // end of the crop area (mipmap level 0)
        public Float3? End
        {
            get => end;
            set
            {
                if(value == end) return;
                end = value;
                if(isEnabled)
                    OnHasChanged();
            }
        }


        private int cropLayer = -1;

        public int Layer
        {
            get => cropLayer;
            set
            {
                if(value == cropLayer) return;
                cropLayer = value;
                if(isEnabled)
                    OnHasChanged();
            }
        }


        private struct BufferData
        {
            public Float3 Start;
            public int CropLayer;
            public Float3 End;
            public float Depth;
            public int Layer;
        }

        public override bool HasWork
        {
            get
            {
                if (!isEnabled) return false;

                // everything gray?
                if (!Start.HasValue || !End.HasValue) return true;
                if (Layer != -1) return true;

                return Start != Float3.Zero || End != Float3.One;
            }
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
                data.CropLayer = Layer;

                // adjust float boundaries to pixel boundaries
                var istart = Start.Value.ToPixels(size);
                var iend = End.Value.ToPixels(size) + Size3.One;
                for (int i = 0; i < 3; ++i)
                {
                    data.Start[i] = (float) istart[i] / size[i];
                    data.End[i] = (float) iend[i] / size[i];
                }
            }

            cbuffer.SetData(data);
            var dev = Device.Get();
            quad.Bind(images.Is3D);
            dev.Pixel.Set(images.Is3D ? Shader3D.Pixel : Shader.Pixel);
            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);

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
    float3 coord = float3(i.texcoord, 0.5);
#if {builder.Is3DInt}
    coord.z = (i.depth + 0.5) / depth;
#endif
    
	if((cropLayer != -1 && cropLayer != layer) // gray all out if on the wrong layer
    || coord.x < start.x || coord.x > end.x // otherwise, gray out based on crop rectangle
    || coord.y < start.y || coord.y > end.y
    || coord.z < start.z || coord.z > end.z)
        return float4(0.0, 0.0, 0.0, 0.5); // gray out this area
    
    discard;
    return 0.0;
}}
";
        }
    }
}
