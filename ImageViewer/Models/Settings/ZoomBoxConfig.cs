using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;

namespace ImageViewer.Models.Settings
{
    public class ZoomBoxConfig
    {
        public class Box
        {
            public float StartX { get; set; }
            public float StartY { get; set; }
            public float EndX { get; set; }
            public float EndY { get; set; }

            public Color Color { get; set; }

            public int Border { get; set; }
        }

        public List<Box> Boxes { get; } = new List<Box>();

        public static ZoomBoxConfig LoadFromModels(ModelsEx models)
        {
            var res = new ZoomBoxConfig();
            foreach (var box in models.ZoomBox.Boxes)
            {
                res.Boxes.Add(new Box
                {
                    StartX = box.Start.X,
                    StartY = box.Start.Y,
                    EndX = box.End.X,
                    EndY = box.End.Y,
                    Border = box.Border,
                    Color = box.Color
                });
            }

            return res;
        }

        public void ApplyToModels(ModelsEx models)
        {
            models.ZoomBox.Boxes.Clear();
            foreach (var box in Boxes)
            {
                models.ZoomBox.Boxes.Add(new BoxOverlay.Box
                {
                    Start = new Float2(box.StartX, box.StartY),
                    End = new Float2(box.EndX, box.EndY),
                    Border = box.Border,
                    Color = box.Color
                });
            }
        }
    }
}
