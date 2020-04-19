using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Overlays;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Color = ImageFramework.Utility.Color;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    using Texture3D = ImageFramework.DirectX.Texture3D;

    public class VolumeView : Texture3DBaseView
    {
        private readonly SmoothVolumeShader smooth;
        private readonly CubeVolumeShader cube;
        private readonly EmptySpaceSkippingShader emptySpaceSkippingShader;
        private readonly CubeSkippingShader cubeSkippingShader;
        private RayCastingDisplayModel displayEx;

        private HelpTextureData[] helpTextures;
        private ITextureCache textureCache;

        public VolumeView(ModelsEx models) : base(models)
        {
            smooth = new SmoothVolumeShader(models);
            cube = new CubeVolumeShader(models);
            emptySpaceSkippingShader = new EmptySpaceSkippingShader();
            cubeSkippingShader = new CubeSkippingShader();
            displayEx = (RayCastingDisplayModel)models.Display.ExtendedViewData;
            helpTextures = new HelpTextureData[models.NumPipelines];
            textureCache = new ImageModelTextureCache(models.Images, Format.R8_UInt, true, false);
        }

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            base.Draw(id, texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = models.ViewData.AlphaDarkenState;
            
            if (models.Display.LinearInterpolation)
            {
                smooth.Run(GetWorldToImage(),
                texture.GetSrView(models.Display.ActiveLayerMipmap), helpTextures[id].GetView(models.Display.ActiveMipmap));
            }
            else
            {
                cube.Run(GetWorldToImage(),
                displayEx.FlatShading, texture.GetSrView(models.Display.ActiveLayerMipmap), helpTextures[id].GetView(models.Display.ActiveMipmap));
            }

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;

            using (var draw = models.Window.SwapChain.Draw.Begin())
            {
                using (var t = draw.SetCanonical(new Float2(20.0f), new Float2(150.0f)))
                {
                    var rot = GetRotation();


                    Sphere3DOverlay.Draw(draw,
                          new Float3(rot.M11, rot.M21, rot.M31),
                          new Float3(rot.M12, rot.M22, rot.M32),
                          new Float3(rot.M13, rot.M23, rot.M33), models.Settings.FlipYAxis);
                }
            }
        }

        private Matrix GetWorldToImage()
        {
            var center = GetCubeCenter();
            var size = models.Images.Size.GetMip(models.Display.ActiveMipmap);

            return
                Matrix.Translation(center.X, center.Y, center.Z) * // to cube center
                GetRotation() * // rotate
                Matrix.Translation(size.X * 0.5f, size.Y * 0.5f, size.Z * 0.5f); // to cube origin
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            // find first valid image
            foreach (var tex in helpTextures)
            {
                if(tex == null) continue;

                // found valid texture
                var res = cube.GetIntersection(GetWorldToImage(), mouse, tex.GetView(models.Display.ActiveMipmap));
                if(res.X < 0) // no intersection at all (what to do?)
                    continue;

                return res;
            }
            
            return Size3.Zero;
        }

        public override void Dispose()
        {
            smooth?.Dispose();
            cube?.Dispose();
            emptySpaceSkippingShader?.Dispose();
            cubeSkippingShader?.Dispose();
            foreach (var tex in helpTextures)
            {
                tex?.Dispose();
            }

            textureCache.Dispose();
        }


        public override void UpdateImage(int id, ITexture texture)
        {
            base.UpdateImage(id, texture);

            if(helpTextures[id] != null)
                helpTextures[id].Dispose();
            helpTextures[id] = null;
            if (texture is null) return;

            helpTextures[id] = new HelpTextureData(this, texture);
        }

        private class HelpTextureData : IDisposable
        {
            private readonly VolumeView parent;
            private readonly Texture3D texture;
            private readonly ITexture srcTexture;
            private bool useLinearInterpolation;
            private readonly List<int> computedMips = new List<int>();

            public HelpTextureData(VolumeView parent, ITexture src)
            {
                this.parent = parent;
                this.srcTexture = src;
                useLinearInterpolation = parent.models.Display.LinearInterpolation;
                texture = (Texture3D)parent.textureCache.GetTexture();
            }

            /// <summary>
            /// returns view of the mipmap (will be computed if not already computed)
            /// </summary>
            public ShaderResourceView GetView(int mipmap)
            {
                if (useLinearInterpolation != parent.models.Display.LinearInterpolation)
                {
                    // reset data
                    computedMips.Clear();
                    useLinearInterpolation = parent.models.Display.LinearInterpolation;
                }

                if (computedMips.Contains(mipmap)) return texture.GetSrView(mipmap);

                // execute shader
                var tmpTex = parent.textureCache.GetTexture();

                if (useLinearInterpolation)
                {
                    parent.emptySpaceSkippingShader.Run(srcTexture, texture, tmpTex, new LayerMipmapSlice(0, mipmap), parent.models.SharedModel.Upload);
                }
                else
                {
                    parent.cubeSkippingShader.Run(srcTexture, texture, tmpTex, new LayerMipmapSlice(0, mipmap), parent.models.SharedModel.Upload);
                }

                parent.textureCache.StoreTexture(tmpTex);
                computedMips.Add(mipmap);

                return texture.GetSrView(mipmap);
            }

            public void Dispose()
            {
                parent.textureCache.StoreTexture(texture);
            }
        }
    }
}
