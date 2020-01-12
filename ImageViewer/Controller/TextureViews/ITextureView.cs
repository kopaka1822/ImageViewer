using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    interface ITextureView : IDisposable
    {
        void Draw(int id, ITexture texture);
        void OnScroll(float amount, Vector2 mouse);
        void OnDrag(Vector2 diff);

        void OnDrag2(Vector2 diff);
        Size3 GetTexelPosition(Vector2 mouse);

        /// indicates that a pipeline image was changed.
        /// assume that initially all textures are null.
        /// <param name="id">id of the images [0, models.NumPipelines-1]</param>
        /// <param name="texture">new texture or null if removed</param>
        void UpdateImage(int id, ITexture texture);
    }
}
