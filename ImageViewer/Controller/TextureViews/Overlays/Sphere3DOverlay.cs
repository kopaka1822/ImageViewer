using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.DirectX;

namespace ImageViewer.Controller.TextureViews.Overlays
{
    public static class Sphere3DOverlay
    {
        /// <summary>
        /// draws a 3d coordinate system overlay in the canonical volume
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="flipY"></param>
        public static void Draw(Direct2D.Context draw, Float3 x, Float3 y, Float3 z, bool flipY)
        {
            draw.FillCircle(Float2.Zero, 1.0f, new Color(0.5f));

            // do some coordinate shuffling because of weird coordinate systems
            y = -y;
            z = -z;
            x = -x;

            x.X = -x.X;
            y.X = -y.X;
            z.X = -z.X;

            if (flipY) y = -y;

            var draws = new List<Drawable>
            {
                new Drawable(x, xColor, "x"),
                new Drawable(-x, xColor, null),
                new Drawable(y, yColor, "y"),
                new Drawable(-y, yColor, null),
                new Drawable(z, zColor, "z"),
                new Drawable(-z, zColor, null),
            };

            draws.Sort();

            foreach (var drawable in draws)
            {
                drawable.Draw(draw);
            }
        }

        private static readonly Color xColor = new Color(1.0f, 0.0f, 0.0f);
        private static readonly Color yColor = new Color(0.0f, 1.0f, 0.0f);
        private static readonly Color zColor = new Color(0.0f, 0.0f, 1.0f);
        private static readonly float stroke = 0.05f;
        private static readonly float radius = 0.8f;
        private static readonly float circleRadius = 0.15f;
        private static readonly float textRadius = 0.08f;
        private static readonly float textStroke = 0.03f;

        private class Drawable : IComparable<Drawable>
        {
            private readonly Float3 vector;
            private readonly Color color;
            [CanBeNull] private readonly string label;

            public Drawable(Float3 vector, Color color, [CanBeNull] string label)
            {
                this.vector = vector;
                this.color = color;
                // darken color 
                if(vector.Z < 0.0f) 
                    this.color = new Color(color.Red * 0.5f, color.Green * 0.5f, color.Blue * 0.5f);
                this.label = label;
            }

            public void Draw(Direct2D.Context draw)
            {
                float labelThreshold = -0.05f;
                var lineColor = new Color(color.Red * 0.7f, color.Green * 0.7f, color.Blue * 0.7f);

                var center = vector.XY * radius;
                if (label != null && vector.Z >= labelThreshold)
                    draw.Line(Float2.Zero, center, stroke, lineColor);
                
                draw.FillCircle(center, circleRadius, color);

                if (label == null) return;

                var tl = center + new Float2(-textRadius, textRadius);
                var tr = center + new Float2(textRadius, textRadius);
                var bl = center + new Float2(-textRadius, -textRadius);
                var br = center + new Float2(textRadius, -textRadius);

                switch (label)
                {
                    case "x":
                        draw.Line(tl, br, textStroke, Colors.Black);
                        draw.Line(tr, bl, textStroke, Colors.Black);
                        break;
                    case "y":
                        draw.Line(center, tl, textStroke, Colors.Black);
                        draw.Line(center, tr, textStroke, Colors.Black);
                        draw.Line(center, center - new Float2(0.0f, textRadius), textStroke, Colors.Black);
                        break;
                    case "z":
                        draw.Line(tl, tr, textStroke, Colors.Black);
                        draw.Line(tr, bl, textStroke, Colors.Black);
                        draw.Line(bl, br, textStroke, Colors.Black);
                        break;
                }

                if (label != null && vector.Z <= labelThreshold)
                    draw.Line(Float2.Zero, center, stroke, lineColor);
            }

            public int CompareTo(Drawable other)
            {
                if (other.vector.Z < vector.Z) return 1;
                return -1;
            }
        }
    }
}
