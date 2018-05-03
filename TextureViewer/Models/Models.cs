using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models
{
    /// <summary>
    /// container for all models
    /// </summary>
    public class Models
    {
        /// <summary>
        /// window after the opengl host was initialized
        /// </summary>
        /// <param name="window"></param>
        public Models(MainWindow window)
        {
            GlContext = new OpenGlContext(window);
            GlContext.Enable();

            GlData = new OpenGlModel(GlContext);
            Images = new ImagesModel(GlContext);
            Display = new DisplayModel(Images);

            GlContext.Disable();
        }

        public void Dispose()
        {
            GlContext.Enable();

            Images.Dispose();
            GlData.Dispose();

            GlContext.Disable();
        }

        public OpenGlContext GlContext { get; private set; }
        public OpenGlModel GlData { get; private set; }
        public ImagesModel Images { get; private set; }
        public DisplayModel Display { get; private set; }
    }
}
