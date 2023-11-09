using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using ImageFramework.DirectX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace ImageViewer.DirectX
{
    public class SwapChain : IDisposable
    {
        private SharpDX.DXGI.SwapChain chain;
        private readonly int bufferCount = 2;
        private readonly SwapChainFlags flags = SwapChainFlags.None;
        private RenderTargetView curView;
        private Texture2D curTarget;
        private Direct2D curDraw;
        private bool isHdr = false; // keeps track if the rtv is hdr
        private readonly IntPtr hwnd;

        public int Width { get; private set; }
        public int Height { get; private set; }

        // retrieves current direct2D draw surface
        public Direct2D Draw
        {
            get
            {
                Debug.Assert(curTarget != null);
                return curDraw ?? (curDraw = new Direct2D(curTarget));
            }
        }

        public SwapChain(IntPtr hwnd, int width, int height)
        {
            this.hwnd = hwnd;
            Initialize(width, height, false);
        }

        void Initialize(int width, int height, bool useHdr)
        {
            Width = width;
            Height = height;
            this.isHdr = useHdr;

            var device = ImageFramework.DirectX.Device.Get();

            var format = Format.R8G8B8A8_UNorm;
            if (useHdr) format = Format.R16G16B16A16_Float;

            var desc = new SwapChainDescription
            {
                BufferCount = bufferCount,
                Flags = flags,
                IsWindowed = true,
                ModeDescription = new ModeDescription(width, height, new Rational(0, 1), format),
                OutputHandle = hwnd,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipSequential,
                Usage = Usage.RenderTargetOutput
            };

            chain = new SharpDX.DXGI.SwapChain(device.FactoryHandle, device.Handle, desc);
        }

        /// <summary>
        /// current render target view
        /// </summary>
        public RenderTargetView Rtv
        {
            get
            {
                Debug.Assert(curView != null);
                return curView;
            }
        }

        public bool IsDisposed => chain.IsDisposed;

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            chain.ResizeBuffers(bufferCount, width, height, Format.Unknown, flags);
        }

        public void BeginFrame(bool useHdr)
        {
            Debug.Assert(curView == null);
            Debug.Assert(curTarget == null);

            if (isHdr != useHdr)
            {
                Dispose();
                Initialize(Width, Height, useHdr);
            }

            curTarget = chain.GetBackBuffer<Texture2D>(0);
            curView = new RenderTargetView(ImageFramework.DirectX.Device.Get().Handle, curTarget);
        }

        public void EndFrame()
        {
            chain.Present(1, PresentFlags.None);
            curView?.Dispose();
            curView = null;
            curDraw?.Dispose();
            curDraw = null;
            curTarget?.Dispose();
            curTarget = null;
        }

        public void Dispose()
        {
            chain?.Dispose();
        }
    }
}
