using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SharpDX.DXGI;
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

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            base.Draw(id, texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = data.AlphaDarkenState;

            if (models.Display.LinearInterpolation)
            {
                shader.Run(data.Buffer, models.Display.ClientAspectRatio, GetWorldToImage(), models.Display.Multiplier, CalcFarplane(), models.Display.DisplayNegative,
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap), helpTextures[id].GetSrView(0));
            }
            else
            {
                marchingShader.Run(data.Buffer, models.Display.ClientAspectRatio, GetWorldToImage(), models.Display.Multiplier, CalcFarplane(), models.Display.DisplayNegative,
                displayEx.FlatShading, texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap), helpTextures[id].GetSrView(0));
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

            if (!(texture is null))
            {
                SpaceSkippingTexture3D tex = new SpaceSkippingTexture3D(texture.Size, texture.NumMipmaps);
                helpTextures[id] = tex;
                emptySpaceSkippingShader.Execute(texture.GetSrView(0, 0), helpTextures[id], texture.Size);
            }
            else
            {
                helpTextures[id].Dispose();
            }
        }

        public class SpaceSkippingTexture3D : TextureBase<SpaceSkippingTexture3D>
        {
            public Size3 texSize;
            public int numMipMaps;
            private readonly SharpDX.Direct3D11.Texture3D handle;

            public override bool Is3D => true;

            public SpaceSkippingTexture3D(Size3 orgSize, int numMipMaps)
            {
                this.texSize = orgSize;
                this.numMipMaps = numMipMaps;

                var desc = new Texture3DDescription
                {
                    Width = texSize.Width,
                    Height = texSize.Height,
                    Depth = texSize.Depth,
                    Format = SharpDX.DXGI.Format.R8_UInt,
                    MipLevels = numMipMaps,
                    BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Default
                };
                handle = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, desc);

                views = new ShaderResourceView[numMipMaps];
                uaViews = new UnorderedAccessView[numMipMaps];

                //Create Views
                for (int curMip = 0; curMip < numMipMaps; ++curMip)
                {
                    views[curMip] = new ShaderResourceView(Device.Get().Handle, GetHandle(), new ShaderResourceViewDescription
                    {
                        Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D,
                        Format = SharpDX.DXGI.Format.R8_UInt,
                        Texture3D = new ShaderResourceViewDescription.Texture3DResource
                        {
                            MipLevels = 1,
                            MostDetailedMip = curMip
                        }
                    });
                    uaViews[curMip] = new UnorderedAccessView(Device.Get().Handle, GetHandle(), new UnorderedAccessViewDescription
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
                Debug.Assert(mipmap >= 0);
                Debug.Assert(mipmap < NumMipmaps);
                return views[mipmap];
            }

            protected override SharpDX.Direct3D11.Resource GetStagingTexture(int layer, int mipmap)
            {
                var mipDim = Size.GetMip(mipmap);
                var desc = new Texture3DDescription
                {
                    Width = mipDim.Width,
                    Height = mipDim.Height,
                    Depth = mipDim.Depth,
                    Format = Format,
                    MipLevels = 1,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Staging
                };

                // create staging texture
                var staging = new SharpDX.Direct3D11.Texture3D(Device.Get().Handle, desc);

                // copy data to staging texture
                Device.Get().CopySubresource(handle, staging, GetSubresourceIndex(layer, mipmap), 0, mipDim);

                return staging;
            }

            public override SpaceSkippingTexture3D CreateT(int numLayer, int numMipmaps, Size3 size, Format format, bool createUav)
            {
                return new SpaceSkippingTexture3D(size, numMipMaps);
            }

            protected override SharpDX.Direct3D11.Resource GetHandle()
            {
                return handle;
            }
        }

    }
}
