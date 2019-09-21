using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace ImageFramework.DirectX
{
    public class TextureArray2D : IDisposable
    {
        public int Width { get; }
        public int Height { get; }
        public int NumMipmaps { get; }
        public bool HasMipmaps => NumMipmaps > 1;
        public int NumLayers { get; }
        public bool IsSrgb { get; } = false;

        private readonly Texture2D handle;
        private ShaderResourceView view;
        private ShaderResourceView[] views;
        private Format format;

        public TextureArray2D(int numLayer, int numMipmaps, int width, int height, Format format, bool isSrgb)
        {
            Width = width;
            Height = height;
            NumMipmaps = numMipmaps;
            NumLayers = numLayer;
            IsSrgb = isSrgb;
            this.format = format;

            var desc = new Texture2DDescription
            { 
                ArraySize = NumLayers,
                BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget, // render target required for mip map generation
                CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
                Format = format,
                Height = height,
                MipLevels = NumMipmaps,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = width
            };
            handle = new SharpDX.Direct3D11.Texture2D(Device.Get().Handle, desc);

            CreateTextureViews();
        }

        public void Dispose()
        {
            handle?.Dispose();
        }

        private void CreateTextureViews()
        {
            Debug.Assert(handle != null);

            // default view
            view = new ShaderResourceView(Device.Get().Handle, handle);

            // single slice views
            views = new ShaderResourceView[NumLayers * NumMipmaps];
            for (int curLayer = 0; curLayer < NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < NumMipmaps; ++curMipmap)
                {
                    var desc = new ShaderResourceViewDescription
                    {
                        Dimension = ShaderResourceViewDimension.Texture2DArray,
                        Format = format,
                        Texture2DArray = new ShaderResourceViewDescription.Texture2DArrayResource
                        {
                            MipLevels = 1,
                            MostDetailedMip = curMipmap,
                            ArraySize = 1,
                            FirstArraySlice = curLayer
                        }
                    };

                    views[GetTextureIndex(curLayer, curMipmap)] = new ShaderResourceView(Device.Get().Handle, handle, desc);
                }
            }
        }

        private int GetTextureIndex(int layer, int mipmap)
        {
            return layer * NumMipmaps + mipmap;
        }
    }
}
