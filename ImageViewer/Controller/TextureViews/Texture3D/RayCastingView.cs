using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;
using SharpDX.Direct3D11;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    using Texture3D = SharpDX.Direct3D11.Texture3D;

    public class RayCastingView : Texture3DBaseView
    {

        private readonly RayCastingShader shader;
        private readonly RayMarchingShader marchingShader;
        private readonly EmptySpaceSkippingShader emptySpaceSkippingShader;
        private RayCastingDisplayModel displayEx;

        private SpaceSkippingTexture3D[] helpTextures;

        public RayCastingView(ModelsEx models, TextureViewData data) : base(models, data)
        {
            shader = new RayCastingShader();
            marchingShader = new RayMarchingShader();
            emptySpaceSkippingShader = new EmptySpaceSkippingShader();
            displayEx = (RayCastingDisplayModel)models.Display.ExtendedViewData;
            helpTextures = new SpaceSkippingTexture3D[4];
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            base.Draw(texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = data.AlphaDarkenState;

            if (models.Display.LinearInterpolation)
            {
                shader.Run(data.Buffer, models.Display.ClientAspectRatio, GetWorldToImage(), models.Display.Multiplier, CalcFarplane(), models.Display.DisplayNegative,
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap));
            }
            else
            {
                marchingShader.Run(data.Buffer, models.Display.ClientAspectRatio, GetWorldToImage(), models.Display.Multiplier, CalcFarplane(), models.Display.DisplayNegative,
                displayEx.FlatShading, texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap), helpTextures[0].GetSrView(0));
            }

            dev.OutputMerger.BlendState = data.DefaultBlendState;
        }

        private Matrix GetWorldToImage()
        {
            float aspectX = models.Images.Size.X / (float)models.Images.Size.Y;
            float aspectZ = models.Images.Size.Z / (float)models.Images.Size.Y;

            return
                Matrix.Translation(-cubeOffsetX, -cubeOffsetY, -GetCubeCenter()) * // translate cube center to origin
                GetRotation() * // undo rotation
                Matrix.Scaling(0.5f * aspectX, 0.5f, 0.5f * aspectZ) * // scale to [-0.5, 0.5]
                Matrix.Translation(0.5f, 0.5f, 0.5f); // move to [0, 1]
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            return new Size3(0, 0, 0);
        }

        public override void Dispose()
        {
            shader?.Dispose();
            marchingShader?.Dispose();
            emptySpaceSkippingShader?.Dispose();
        }

        public override void UpdateImage(int id, ITexture texture)
        {
            base.UpdateImage(id, texture);
            SpaceSkippingTexture3D tex = new SpaceSkippingTexture3D(id, texture);
            helpTextures[id] = tex;

            emptySpaceSkippingShader.Execute(texture.GetSrView(0, 0), helpTextures[id].GetUaView(0),texture.Size);
            
        }

        private class SpaceSkippingTexture3D
        {
            private UnorderedAccessView[] uaViews;
            private ShaderResourceView[] srViews;
            private Texture3D texHandle;
            public SpaceSkippingTexture3D(int id, ITexture orgTex)
            {
                Size3 texSize = orgTex.Size;
                int NumMipmaps = orgTex.NumMipmaps;

                var desc = new Texture3DDescription
                {
                    Width = texSize.Width,
                    Height = texSize.Height,
                    Depth = texSize.Depth,
                    Format = SharpDX.DXGI.Format.R8_UInt,
                    MipLevels = orgTex.NumMipmaps,
                    BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Default
                };
                texHandle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, desc);
                
                srViews = new ShaderResourceView[NumMipmaps];
                uaViews = new UnorderedAccessView[NumMipmaps];

                //Create Views
                for (int curMip = 0; curMip < NumMipmaps; ++curMip)
                {
                    srViews[curMip] = new ShaderResourceView(Device.Get().Handle, texHandle, new ShaderResourceViewDescription
                    {
                        Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D,
                        Format = SharpDX.DXGI.Format.R8_UInt,
                        Texture3D = new ShaderResourceViewDescription.Texture3DResource
                        {
                            MipLevels = 1,
                            MostDetailedMip = curMip
                        }
                    });
                    uaViews[curMip] = new UnorderedAccessView(Device.Get().Handle, texHandle, new UnorderedAccessViewDescription
                    {
                        Dimension = UnorderedAccessViewDimension.Texture3D,
                        Format = SharpDX.DXGI.Format.R8_UInt,
                        Texture3D = new UnorderedAccessViewDescription.Texture3DResource
                        {
                            FirstWSlice = 0,
                            MipSlice = curMip,
                            WSize = -1 // all slices
                        }
                    });
                }

            }
            public ShaderResourceView GetSrView(int mipmap)
            {
                return srViews[mipmap];
            }
            public UnorderedAccessView GetUaView(int mipmap)
            {
                return uaViews[mipmap];
            }

        }

    }
}
