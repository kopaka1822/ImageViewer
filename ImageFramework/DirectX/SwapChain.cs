using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace ImageFramework.DirectX
{
    public class SwapChain : IDisposable
    {
        private readonly SharpDX.DXGI.SwapChain chain;
        private readonly int bufferCount = 2;
        private readonly SwapChainFlags flags = SwapChainFlags.None;
        private float col = 0.0f;

        public SwapChain(IntPtr hwnd, int width, int height)
        {
            var desc = new SwapChainDescription
            {
                BufferCount = bufferCount,
                Flags = flags,
                IsWindowed = true,
                ModeDescription = new ModeDescription(width, height, new Rational(0, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = hwnd,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            var device = Device.Get();
            chain = new SharpDX.DXGI.SwapChain(device.FactoryHandle, device.Handle, desc);
        }

        public void Resize(int width, int height)
        {
            chain.ResizeBuffers(bufferCount, width, height, Format.Unknown, flags);
        }

        public void BeginFrame()
        {
            using (var backBuffer = chain.GetBackBuffer<Texture2D>(0))
            {
                using (var renderTarget = new RenderTargetView(Device.Get().Handle, backBuffer))
                {
                    col = 1.0f - col;
                    Device.Get().ClearRenderTargetView(renderTarget, new RawColor4(col, 0.0f, 0.0f, 1.0f));
                }
            }
           
        }

        public void EndFrame()
        {
            chain.Present(1, PresentFlags.None);
            
        }

        public void Dispose()
        {
            chain?.Dispose();
        }
    }
}
