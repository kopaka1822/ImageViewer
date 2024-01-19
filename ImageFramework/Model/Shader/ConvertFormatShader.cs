using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Scaling;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = ImageFramework.ImageLoader.Resource;
using Texture3D = ImageFramework.DirectX.Texture3D;

namespace ImageFramework.Model.Shader
{
    public class ConvertFormatShader : IDisposable
    {
        private readonly DirectX.Shader convert2D;
        private readonly DirectX.Shader convert3D;
        private DirectX.Shader convert2DInt;
        private DirectX.Shader convert3DInt;
        private readonly QuadShader quad;
        private readonly UploadBuffer cbuffer;

        public ConvertFormatShader(QuadShader quad, UploadBuffer upload)
        {
            var dev = DirectX.Device.Get();
            convert2D = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(ShaderBuilder.Builder2D), "ConvertFormatShader2D");
            convert3D = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(ShaderBuilder.Builder3D), "ConvertFormatShader3D");
            this.quad = quad;
            cbuffer = upload;
        }

        /// <summary>
        /// copies a single mip from one layer of a texture to another layer of a texture. The formats don't have to match
        /// </summary>
        /// <param name="src">source texture</param>
        /// <param name="dst">destination texture</param>
        public void CopyLayer(ITexture src, LayerMipmapSlice srcLm, ITexture dst, LayerMipmapSlice dstLm)
        {
            Debug.Assert(src.Size == dst.Size);

            var dev = DirectX.Device.Get();
            quad.Bind(src.Is3D);
            if (src.Is3D) dev.Pixel.Set(convert3D.Pixel);
            else dev.Pixel.Set(convert2D.Pixel);

            dev.Pixel.SetShaderResource(0, src.View);
 
            cbuffer.SetData(new LayerLevelOffsetData
            {
                Layer = srcLm.Layer,
                Level = srcLm.Mipmap,
                Xoffset = 0,
                Yoffset = 0,
                Multiplier = 1.0f,
                UseOverlay = 0,
                Scale = 1
            });

            var dim = dst.Size.GetMip(dstLm.Mipmap);
            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.OutputMerger.SetRenderTargets(dst.GetRtView(dstLm));
            dev.SetViewScissors(dim.Width, dim.Height);
            dev.DrawFullscreenTriangle(dim.Depth);

            // remove bindings
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();
        }

        /// <summary>
        /// converts the texture into another format and performs cropping if requested
        /// </summary>
        /// <param name="texture">source texture</param>
        /// <param name="dstFormat">destination format</param>
        /// <param name="scaling">required for regenerating mipmaps after cropping</param>
        /// <param name="mipmap">mipmap to export, -1 for all mipmaps</param>
        /// <param name="layer">layer to export, -1 for all layers</param>
        /// <param name="multiplier">rgb channels will be multiplied by this value</param>
        public ITexture Convert(ITexture texture, SharpDX.DXGI.Format dstFormat, ScalingModel scaling, int mipmap = -1, int layer = -1, float multiplier = 1.0f)
       {
            return Convert(texture, dstFormat, new LayerMipmapRange(layer, mipmap), multiplier, false, Size3.Zero, Size3.Zero, Size3.Zero, scaling, null);
       }

        /// <summary>
        /// converts the texture into another format and performs cropping if requested
        /// </summary>
        /// <param name="texture">source texture</param>
        /// <param name="dstFormat">destination format</param>
        /// <param name="srcLm">layer/mipmap to export</param>
        /// <param name="multiplier">rgb channels will be multiplied by this value</param>
        /// <param name="crop">indicates if the image should be cropped, only works with 1 mipmap to export</param>
        /// <param name="offset">if crop: offset in source image</param>
        /// <param name="size">if crop: size of the destination image</param>
        /// <param name="align">if nonzero: texture width will be aligned to this (rounded down)</param>
        /// <param name="scaling">required for regenerating mipmaps after cropping.</param>
        /// <param name="overlay">overlay</param>
        /// <param name="scale">scales the destination image by this factor</param>
        /// <returns></returns>
        public ITexture Convert(ITexture texture, SharpDX.DXGI.Format dstFormat, LayerMipmapRange srcLm, float multiplier, bool crop, 
            Size3 offset, Size3 size, Size3 align, ScalingModel scaling, ITexture overlay = null, int scale = 1)
        {
            Debug.Assert(ImageFormat.IsSupported(dstFormat));
            Debug.Assert(ImageFormat.IsSupported(texture.Format));
            Debug.Assert(overlay == null || texture.HasSameDimensions(overlay));
            Debug.Assert(scale >= 1);

            // set width, height mipmap
            int nMipmaps = srcLm.IsSingleMipmap ? 1: texture.NumMipmaps;
            int nLayer = srcLm.IsSingleLayer ? 1 : texture.NumLayers;

            // set correct width, height, offsets
            if (!crop)
            {
                size = texture.Size.GetMip(srcLm.FirstMipmap);
                offset = Size3.Zero;
            }

            if (scale != 1)
            {
                size.X *= scale;
                size.Y *= scale;
                if (texture.Is3D) size.Z *= scale;
                offset.X *= scale;
                offset.Y *= scale;
                if (texture.Is3D) offset.Z *= scale;
            }

            // adjust alignments
            for (int i = 0; i < 3; ++i)
            {
                if (align[i] != 0)
                {
                    if (size[i] % align[i] != 0)
                    {
                        if (size[i] < align[i])
                            throw new Exception($"image needs to be aligned to {align[i]} but one axis is only {size[i]}. Axis should be at least {align[i]}");

                        crop = true;
                        var remainder = size[i] % align[i];
                        offset[i] = offset[i] + remainder / 2;
                        size[i] = size[i] - remainder;
                    }
                }
            }

            bool recomputeMips = nMipmaps > 1 && (crop || scale != 1);
            if (recomputeMips)
            {
                // number of mipmaps might have changed
                nMipmaps = size.MaxMipLevels;
                recomputeMips = nMipmaps > 1;
            }

            var res = texture.Create(new LayerMipmapCount(nLayer, nMipmaps), size, dstFormat, false);

            var dev = DirectX.Device.Get();
            quad.Bind(texture.Is3D);
            if(texture.Is3D) dev.Pixel.Set(convert3D.Pixel);
            else dev.Pixel.Set(convert2D.Pixel);

            Debug.Assert(texture.View != null);
            dev.Pixel.SetShaderResource(0, texture.View);
            if(overlay != null)
                dev.Pixel.SetShaderResource(1, overlay.View);
            else dev.Pixel.SetShaderResource(1, null);

            foreach (var dstLm in res.LayerMipmap.Range)
            {
                cbuffer.SetData(new LayerLevelOffsetData
                {
                    Layer = dstLm.Layer + srcLm.FirstLayer + offset.Z,
                    Level = dstLm.Mipmap + srcLm.FirstMipmap,
                    Xoffset = offset.X,
                    Yoffset = offset.Y,
                    Multiplier = multiplier,
                    UseOverlay = overlay != null ? 1 : 0,
                    Scale = scale
                });

                var dim = res.Size.GetMip(dstLm.Mipmap);
                dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
                dev.OutputMerger.SetRenderTargets(res.GetRtView(dstLm));
                dev.SetViewScissors(dim.Width, dim.Height);
                dev.DrawFullscreenTriangle(dim.Depth);

                if (recomputeMips && dstLm.Mipmap > 0) break; // only write most detailed mipmap
            }

            // remove bindings
            dev.Pixel.SetShaderResource(0, null);
            dev.Pixel.SetShaderResource(1, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView) null);
            quad.Unbind();

            if (recomputeMips)
            {
                scaling.WriteMipmaps(res);
            }

            return res;
        }

        /// <summary>
        /// takes a list of textures and combines them into an array.
        /// </summary>
        /// <param name="textures">list of images with same dimensions and no layers</param>
        /// <returns>combined image</returns>
        public TextureArray2D CombineToArray(List<TextureArray2D> textures)
        {
            if (textures.Count == 0) return null;
            var first = textures[0];

            // try to find a fitting format
            var bestFormat = first.Format;
            if (bestFormat != Format.R32G32B32A32_Float)
            {
                foreach (var t in textures)
                {
                    if (t.Format != bestFormat)
                    {
                        // different formats => take a format that is supported by all types
                        bestFormat = Format.R32G32B32A32_Float;
                        break;
                    }
                }
            }

            var tex = new TextureArray2D(new LayerMipmapCount(textures.Count, first.NumMipmaps), first.Size, bestFormat, false);
            for (int i = 0; i < textures.Count; ++i)
            {
                for (int curMip = 0; curMip < first.NumMipmaps; ++curMip)
                {
                    CopyLayer(textures[i], new LayerMipmapSlice(0, curMip), tex, new LayerMipmapSlice(i, curMip));
                }
            }

            return tex;
        }

        public List<TextureArray2D> FlattenToArray(TextureArray2D src)
        {
            List<TextureArray2D> res = new List<TextureArray2D>(src.NumLayers);
            
            for (int i = 0; i < src.NumLayers; ++i)
            {
                var dst = new TextureArray2D(new LayerMipmapCount(1, src.NumMipmaps), src.Size, src.Format, false);

                for (int curMip = 0; curMip < src.NumMipmaps; ++curMip)
                {
                    CopyLayer(src, new LayerMipmapSlice(i, curMip), dst, new LayerMipmapSlice(0, curMip));
                }

                res.Add(dst);
            }

            return res;
        }

        /// <summary>
        /// for unit testing purposes. Converts naked srv to TextureArray2D
        /// </summary>
        internal TextureArray2D ConvertFromRaw(SharpDX.Direct3D11.ShaderResourceView srv, Size3 size, SharpDX.DXGI.Format dstFormat, bool isInteger)
        {
            var res = new TextureArray2D(LayerMipmapCount.One, size, dstFormat, false);

            var dev = DirectX.Device.Get();
            quad.Bind(false);
            if (isInteger)
            {
                if(convert2DInt == null) convert2DInt = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(new ShaderBuilder2D("int4")), "ConvertInt");
                dev.Pixel.Set(convert2DInt.Pixel);
            }
            else dev.Pixel.Set(convert2D.Pixel);

            dev.Pixel.SetShaderResource(0, srv);

            cbuffer.SetData(new LayerLevelOffsetData
            {
                Layer = 0,
                Level = 0,
                Xoffset = 0,
                Yoffset = 0,
                Multiplier = 1.0f,
                UseOverlay = 0,
                Scale = 1
            });

            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.OutputMerger.SetRenderTargets(res.GetRtView(LayerMipmapSlice.Mip0));
            dev.SetViewScissors(size.Width, size.Height);
            dev.DrawQuad();

            // remove bindings
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();

            return res;
        }

        /// <summary>
        /// for unit testing purposes. Converts naked srv to TextureArray2D
        /// </summary>
        internal Texture3D ConvertFromRaw3D(SharpDX.Direct3D11.ShaderResourceView srv, Size3 size, SharpDX.DXGI.Format dstFormat, bool isInteger)
        {
            var res = new Texture3D(1, size, dstFormat, false);

            var dev = DirectX.Device.Get();
            quad.Bind(true);
            if (isInteger)
            {
                if(convert3DInt == null) convert3DInt = new DirectX.Shader(DirectX.Shader.Type.Pixel, GetSource(new ShaderBuilder3D("int4")), "ConvertInt");
                dev.Pixel.Set(convert3DInt.Pixel);
            }
            else dev.Pixel.Set(convert3D.Pixel);

            dev.Pixel.SetShaderResource(0, srv);

            cbuffer.SetData(new LayerLevelOffsetData
            {
                Layer = 0,
                Level = 0,
                Xoffset = 0,
                Yoffset = 0,
                Multiplier = 1.0f,
                UseOverlay = 0,
                Scale = 1
            });

            dev.Pixel.SetConstantBuffer(0, cbuffer.Handle);
            dev.OutputMerger.SetRenderTargets(res.GetRtView(LayerMipmapSlice.Mip0));
            dev.SetViewScissors(size.Width, size.Height);
            dev.DrawFullscreenTriangle(size.Depth);

            // remove bindings
            dev.Pixel.SetShaderResource(0, null);
            dev.OutputMerger.SetRenderTargets((RenderTargetView)null);
            quad.Unbind();

            return res;
        }

        private static string GetSource(IShaderBuilder builder)
        {
            return $@"
{builder.SrvType} in_tex : register(t0);
{builder.SrvType} in_overlay : register(t1);

cbuffer InfoBuffer : register(b0)
{{
    uint layer; // contains offset z for 3d textures
    uint level;
    uint xoffset;
    uint yoffset;
    float multiplier;
    bool useOverlay;
    uint scale;
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
    uint3 coord = uint3((uint2(i.projPos.xy) + uint2(xoffset, yoffset)) / scale, layer);
#if {builder.Is3DInt}
    coord.z = (i.depth + layer) / scale;
#endif
    float4 color = in_tex.mips[level][coord];
	color.rgb *= multiplier;
    if(useOverlay)
    {{
        // overlay has alpha premultiplied color and alpha channel contains inverse alpha (occlusion)
        float4 overlay = in_overlay.mips[level][coord];
        color.rgb = overlay.rgb + overlay.a * color.rgb;
        color.a = 1.0 - (overlay.a * (1.0 - color.a));
    }}
	return color;
}}
";
        }

        public void Dispose()
        {
            convert2D?.Dispose();
            convert3D?.Dispose();
            convert2DInt?.Dispose();
            convert3DInt?.Dispose();
        }
    }
}
