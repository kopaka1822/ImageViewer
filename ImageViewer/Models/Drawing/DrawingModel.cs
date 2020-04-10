using System;

namespace ImageViewer.Models.Drawing
{
    /// <summary>
    /// common utilities to draw stuff
    /// </summary>
    public class DrawingModel : IDisposable
    {
        public CircleShader Circle { get; }

        public DrawingModel()
        {
            Circle = new CircleShader();
        }

        public void Dispose()
        {
            Circle?.Dispose();
        }
    }
}
