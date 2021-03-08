using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;

namespace ImageViewer.Models.Settings
{
    public class ArrowsConfig
    {
        public class Arrow
        {
            public float StartX { get; set; }
            public float StartY { get; set; }
            public float EndX { get; set; }
            public float EndY { get; set; }
            public Color Color { get; set; }
            public int StrokeWidth { get; set; }
        }

        public List<Arrow> Arrows { get; } = new List<Arrow>();

        public static ArrowsConfig LoadFromModels(ModelsEx models)
        {
            var res = new ArrowsConfig();
            foreach (var a in models.Arrows.Arrows)
            {
                res.Arrows.Add(new Arrow
                {
                    StartX = a.Start.X,
                    StartY = a.Start.Y,
                    EndX = a.End.X,
                    EndY = a.End.Y,
                    StrokeWidth = a.Width,
                    Color = a.Color
                });
            }

            return res;
        }

        public void ApplyToModels(ModelsEx models)
        {
            models.Arrows.Arrows.Clear();
            foreach (var a in Arrows)
            {
                models.Arrows.Arrows.Add(new ArrowOverlay.Arrow
                {
                    Start = new Float2(a.StartX, a.StartY),
                    End = new Float2(a.EndX, a.EndY),
                    Width = a.StrokeWidth,
                    Color = a.Color
                });
            }
        }
    }
}
 