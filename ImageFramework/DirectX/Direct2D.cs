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
using Factory = SharpDX.Direct2D1.Factory;

namespace ImageFramework.DirectX
{
    public class Direct2D : IDisposable
    {
        private class Core
        {
            public Factory Factory { get; }

            public StrokeStyle RoundStroke { get; }
            public StrokeStyle HardStroke { get; }

            public Core()
            {
#if DEBUG
                var debugLevel = DebugLevel.Information;
#else
                var debugLevel = DebugLevel.None;
#endif
                Factory = new SharpDX.Direct2D1.Factory1(FactoryType.MultiThreaded, debugLevel);

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

            /// <summary>
            /// transforms the screen space coordinates into a canonical coordinate system [-1, 1] with y up
            /// </summary>
            /// <param name="start">screen space start</param>
            /// <param name="end">screen space end</param>
            /// <returns>canonical transform</returns>
            public Transform SetCanonical(Float2 start, Float2 end)
            {
                RawMatrix3x2 t = new RawMatrix3x2(
                    (end.X - start.X) * 0.5f, 0.0f, // column 1
                    0.0f, -(end.Y - start.Y) * 0.5f, // column 2
                    (end.X - start.X) * 0.5f + start.X, (end.Y - start.Y) * 0.5f + start.Y // column 3
                );

                return new Transform(parent, t);
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
            if (core == null) core = new Core();

            using (var surface = buffer.QueryInterface<SharpDX.DXGI.Surface>())
            {
                target = new RenderTarget(core.Factory, surface, new RenderTargetProperties
                {
                    DpiX = 0.0f, // use default dpi
                    DpiY = 0.0f,
                    MinLevel = FeatureLevel.Level_10,
                    PixelFormat = new PixelFormat
                    {
                        Format = buffer.Description.Format,
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

        // utility classes
        public class Transform : IDisposable
        {
            private readonly Direct2D parent;
            private readonly RawMatrix3x2 original;

            public Transform(Direct2D parent, RawMatrix3x2 transform)
            {
                this.parent = parent;
                original = parent.target.Transform;
                parent.target.Transform = transform;
            }

            public void Dispose()
            {
                parent.target.Transform = original;
            }
        }

    }
}
