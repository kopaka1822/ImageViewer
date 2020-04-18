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

        private Texture3D[] helpTextures;
        private ITextureCache textureCache;

        public VolumeView(ModelsEx models) : base(models)
        {
            smooth = new SmoothVolumeShader(models);
            cube = new CubeVolumeShader(models);
            emptySpaceSkippingShader = new EmptySpaceSkippingShader();
            cubeSkippingShader = new CubeSkippingShader();
            displayEx = (RayCastingDisplayModel)models.Display.ExtendedViewData;
            helpTextures = new Texture3D[models.NumPipelines];
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
                smooth.Run(GetWorldToImage(), models.Display.ClientAspectRatioScalar, 
                texture.GetSrView(models.Display.ActiveLayerMipmap), helpTextures[id].GetSrView(models.Display.ActiveMipmap));
            }
            else
            {
                cube.Run(GetWorldToImage(), models.Display.ClientAspectRatioScalar,
                displayEx.FlatShading, texture.GetSrView(models.Display.ActiveLayerMipmap), helpTextures[id].GetSrView(models.Display.ActiveMipmap));
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
            return new Size3(0, 0, 0);
        }

        public override void Dispose()
        {
            smooth?.Dispose();
            cube?.Dispose();
            emptySpaceSkippingShader?.Dispose();
            cubeSkippingShader?.Dispose();
            textureCache.Dispose();
            foreach (var tex in helpTextures)
            {
                tex?.Dispose();
            }
        }

        public override void UpdateImage(int id, ITexture texture)
        {
            base.UpdateImage(id, texture);

            if(helpTextures[id] != null)
                textureCache.StoreTexture(helpTextures[id]);
            helpTextures[id] = null;
            if (texture is null) return;

            //Texture3D tex = new Texture3D(texture.NumMipmaps, texture.Size, Format.R8_UInt, true, false);
            helpTextures[id] = (Texture3D)textureCache.GetTexture();
            var tmpTex = textureCache.GetTexture();
            foreach (var lm in texture.LayerMipmap.Range)
            {
                emptySpaceSkippingShader.Run(texture, helpTextures[id], tmpTex, lm, models.SharedModel.Upload);
            }
            textureCache.StoreTexture(tmpTex);
        }

        /*private class HelpTextureData : IDisposable
        {
            private readonly VolumeView parent;
            private readonly Texture3D texture;

            public HelpTextureData(VolumeView parent, ITexture src)
            {
                this.parent = parent;
                
            }

            public void Dispose()
            {

            }
        }*/
    }
}
