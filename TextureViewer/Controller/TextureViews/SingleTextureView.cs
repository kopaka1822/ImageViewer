using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.Models;

namespace TextureViewer.Controller.TextureViews
{
    class SingleTextureView : PlainTextureView
    {
        public SingleTextureView(Models.Models models) : base(models)
        {
        }

        public override void Draw()
        {
            DrawLayer(Matrix4.Identity, models.Display.ActiveLayer);
        }
    }
}
