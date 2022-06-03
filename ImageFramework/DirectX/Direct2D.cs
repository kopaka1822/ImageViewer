using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Factory = SharpDX.Direct2D1.Factory;
using FactoryType = SharpDX.Direct2D1.FactoryType;

namespace ImageFramework.DirectX
{
    public class Direct2D : IDisposable
    {
        private class Core
        {
            public Factory Factory { get; }
            public SharpDX.DirectWrite.Factory WriteFactory { get; }
            public SharpDX.Direct2D1.Device Handle { get; }
            public SharpDX.Direct2D1.DeviceContext Context { get; }

            public StrokeStyle RoundStroke { get; }
            public StrokeStyle HardStroke { get; }

            public TextFormat DefaultText { get; }

            public Core()
            {
                using (var dxgiDevice =
                    ImageFramework.DirectX.Device.Get().Handle.QueryInterface<SharpDX.DXGI.Device>())
                {
#if DEBUG
                    var debugLevel = DebugLevel.Information;
#else
                var debugLevel = DebugLevel.None;
#endif
                    var factory = new SharpDX.Direct2D1.Factory1(FactoryType.MultiThreaded, debugLevel);
                    Factory = factory;
                    Handle = new SharpDX.Direct2D1.Device(factory, dxgiDevice);
                    Context = new SharpDX.Direct2D1.DeviceContext(Handle, DeviceContextOptions.None);
                }

                WriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);

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
                DefaultText?.Dispose();
                WriteFactory?.Dispose();
                Context?.Dispose();
                Handle?.Dispose();
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

            public void Clear(Color color)
            {
                parent.target.Clear(new RawColor4(color.Red, color.Green, color.Blue, color.Alpha));
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

            public void Text(Float2 start, Float2 end, float size, Color color, string text, TextAlignment alignment = TextAlignment.Leading)
            {
                var font = parent.GetFont(size);
                font.TextAlignment = alignment;
                
                parent.target.DrawText(
                    text, 
                    font, 
                    new RawRectangleF(start.X, start.Y, end.X, end.Y),
                    parent.GetBrush(color)
                );
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

            public Clip Clip(Float2 start, Float2 end)
            {
                parent.target.PushAxisAlignedClip(new RawRectangleF(start.X, start.Y, end.X, end.Y), AntialiasMode.Aliased);
                return new Clip(parent.target);
            }

            public void Dispose()
            {
                parent.target.EndDraw();
            }
        }

        private readonly SharpDX.Direct2D1.RenderTarget target;
        private readonly Dictionary<Color, SolidColorBrush> brushes = new Dictionary<Color, SolidColorBrush>();
        private readonly Dictionary<float, TextFormat> fonts = new Dictionary<float, TextFormat>();

        private static Core core;

        // indicates if the given format is a supported render target
        public static bool IsSupported(Format format)
        {
            if(core == null) core = new Core();
            return core.Context.IsDxgiFormatSupported(format);
        }

        public Direct2D(TextureArray2D texture) : this(texture.Handle) {}

        public Direct2D(Texture2D buffer)
        {
            if (core == null) core = new Core();
            
            var format = buffer.Description.Format;

            if (!core.Context.IsDxgiFormatSupported(format))
                throw new Exception("Format " + format + " not supported for direct2D rendertarget");

            // this is required to obtain a surface
            Debug.Assert(buffer.Description.ArraySize == 1);
            Debug.Assert(buffer.Description.MipLevels == 1);

            using (var surface = buffer.QueryInterface<SharpDX.DXGI.Surface>())
            {
                target = new RenderTarget(core.Factory, surface, new RenderTargetProperties
                {
                    DpiX = 96.0f, // use default dpi
                    DpiY = 96.0f,
                    MinLevel = FeatureLevel.Level_10,
                    PixelFormat = new PixelFormat
                    {
                        Format = format,
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

        private TextFormat GetFont(float size)
        {
            if (fonts.TryGetValue(size, out var res))
                return res;

            res = new TextFormat(core.WriteFactory, "Verdana", FontWeight.Normal, FontStyle.Normal, size)
            {
                WordWrapping = WordWrapping.Wrap, 
                ParagraphAlignment = ParagraphAlignment.Near
            };
            // set some defaults
            fonts.Add(size, res);

            return res;
        }

        public void Dispose()
        {
            foreach (var brush in brushes)
            {
                brush.Value.Dispose();
            }
            foreach (var font in fonts)
            {
                font.Value.Dispose();
            }
            target?.Dispose();
        }

        // utility classes

        // undo transform when out of scope
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

        // undo clip when out of scope
        public class Clip : IDisposable
        {
            private readonly RenderTarget parent;

            public Clip(RenderTarget parent)
            {
                this.parent = parent;
            }

            public void Dispose()
            {
                parent.PopAxisAlignedClip();
            }
        }
    }
}
