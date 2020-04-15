using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Device = ImageFramework.DirectX.Device;
using Factory = SharpDX.Direct2D1.Factory;
using FactoryType = SharpDX.Direct2D1.FactoryType;

namespace ImageViewer.DirectX
{
    public class Direct2D : IDisposable
    {
        private class Core
        {
            public Factory Factory { get; }
            public float DpiX { get; }
            public float DpiY { get; }

            public StrokeStyle RoundStroke { get; }
            public StrokeStyle HardStroke { get; }
            
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
                RoundStroke = new StrokeStyle(Factory, new StrokeStyleProperties
                {
                    StartCap = CapStyle.Round,
                    EndCap = CapStyle.Round
                });
                HardStroke = new StrokeStyle(Factory, new StrokeStyleProperties
                {
                    StartCap = CapStyle.Flat,
                    EndCap = CapStyle.Flat
                });

                ImageFramework.DirectX.Device.Get().DeviceDispose += (sender, args) => Dispose();

            }
            private void Dispose()
            {
                //Context?.Dispose();
                //Handle?.Dispose();
                RoundStroke?.Dispose();
                HardStroke?.Dispose();
                Factory?.Dispose();
            }
        }

        // context that can be used for drawing
        public class Context : IDisposable
        {
            private readonly Direct2D parent;

            internal Context(Direct2D parent)
            {
                this.parent = parent;
                Device.Get().Flush();
                parent.target.BeginDraw();
            }

            public void FillRectangle(Float2 start, Float2 end, Color color)
            {
                parent.target.FillRectangle(new RawRectangleF(start.X, start.Y, end.X, end.Y), parent.GetBrush(color));
            }

            public void FillEllipse(Float2 center, float xRadius, float yRadius, Color color)
            {
                parent.target.FillEllipse(new Ellipse
                {
                    Point = new RawVector2(center.X, center.Y),
                    RadiusX = xRadius,
                    RadiusY = yRadius
                }, parent.GetBrush(color));
            }

            public void FillCircle(Float2 center, float radius, Color color)
            {
                FillEllipse(center, radius, radius, color);
            }

            public void Line(Float2 start, Float2 end, float width, Color color, bool round = true)
            {
                parent.target.DrawLine(new RawVector2(start.X, start.Y), new RawVector2(end.X, end.Y), parent.GetBrush(color), width, 
                    round ? Direct2D.core.RoundStroke : Direct2D.core.HardStroke);
            }

            public void Dispose()
            {
                parent.target.EndDraw();
            }
        }

        private readonly SharpDX.Direct2D1.RenderTarget target;
        private readonly Dictionary<Color, SolidColorBrush> brushes = new Dictionary<Color, SolidColorBrush>();
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
        }

        public Context Begin()
        {
            return new Context(this);
        }

        private Brush GetBrush(Color color)
        {
            if (brushes.TryGetValue(color, out var res))
                return res;
            
            res = new SolidColorBrush(target, new RawColor4(color.Red, color.Green, color.Blue, color.Alpha));
            brushes.Add(color, res);

            return res;
        }

        public void Dispose()
        {
            foreach (var brush in brushes)
            {
                brush.Value.Dispose();
            }
            target?.Dispose();
        }
    }
}
