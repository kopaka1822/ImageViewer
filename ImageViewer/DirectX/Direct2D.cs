using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = ImageFramework.DirectX.Device;
using Factory = SharpDX.Direct2D1.Factory;

namespace ImageViewer.DirectX
{
    public class Direct2D : IDisposable
    {
        private class Core : IDisposable
        {
            public Factory Factory { get; }
            public float DpiX { get; }
            public float DpiY { get; }
            //public SharpDX.Direct2D1.Device Handle { get; }
            //public SharpDX.Direct2D1.DeviceContext Context { get; }

            public Core()
            {
                //using (var dxgiDevice = ImageFramework.DirectX.Device.Get().Handle.QueryInterface<SharpDX.DXGI.Device>())
                {
#if DEBUG
                    var debugLevel = DebugLevel.Information;
#else
                var debugLevel = DebugLevel.None;
#endif
                    Factory = new SharpDX.Direct2D1.Factory1(FactoryType.MultiThreaded, debugLevel);
                    //Handle = new SharpDX.Direct2D1.Device(factory, dxgiDevice);
                    //Context = new DeviceContext(Handle, DeviceContextOptions.None);
                }

                DpiX = Factory.DesktopDpi.Width;
                DpiY = Factory.DesktopDpi.Height;

                //Context.BeginDraw();
            }



            public void Dispose()
            {
                //Context?.Dispose();
                //Handle?.Dispose();
                Factory?.Dispose();
            }
        }

        private readonly SharpDX.Direct2D1.RenderTarget target;
        private static Core core;

        public Direct2D(Texture2D buffer)
        {
            if(core == null) core = new Core();

            using (var surface = buffer.QueryInterface<SharpDX.DXGI.Surface>())
            {
                target = new RenderTarget(core.Factory, surface, new RenderTargetProperties
                {
                    DpiX = core.DpiX,
                    DpiY = core.DpiY,
                    MinLevel = FeatureLevel.Level_10,
                    PixelFormat = new PixelFormat
                    {
                        Format = Format.R8G8B8A8_UNorm,
                        AlphaMode = AlphaMode.Ignore
                    },
                    Type = RenderTargetType.Hardware,
                    Usage = RenderTargetUsage.None
                });
            }

            Device.Get().Flush();
            Test();
        }

        void Test()
        {
            target.BeginDraw();

            
            /*target.FillRectangle(new RawRectangleF
            {
                Left = 0.0f, Right = 100.0f,
                Bottom = 0.0f, Top = 100.0f
            }, CreateBrush(new Color(1.0f, 0.0f, 0.0f)));*/

            target.EndDraw();
        }

        private Brush CreateBrush(Color color)
        {
            return new SolidColorBrush(target, new RawColor4(color.Red, color.Green, color.Blue, color.Alpha));
        }

        public void Dispose()
        {
            target?.Dispose();
        }
    }
}
