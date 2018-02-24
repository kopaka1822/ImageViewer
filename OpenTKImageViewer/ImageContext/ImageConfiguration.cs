using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.Utility;

namespace OpenTKImageViewer.ImageContext
{
    public class ImageConfiguration
    {
        private readonly ImageContext parent;
        private readonly ImageCombineShader combineShader;

        private bool useTonemapper = true;
        // helper array for tonemapping
        private TextureArray2D[] pingpong;
        public bool RecomputeImage { get; set; } = true;

        public bool UseTonemapper
        {
            get { return useTonemapper; }
            set {
                if (value == useTonemapper) return;
                useTonemapper = value;
                RecomputeImage = true;
            }
        }
        public IStepable TonemappingStepable { get; private set; }= null;

        // texture until the statistics point. this will can be null if statistics texture would be equal to the display texture
        private TextureArray2D statisticsTexture = null;
        // texture after tonemapping
        public TextureArray2D DisplayTexture { get; private set; }
        public ImageFormula CombineFormula { get; }
        public ImageFormula AlphaFormula { get; }
        public bool Active { get; set; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="defaultImage">default image for the image equation (0 == "I0")</param>
        public ImageConfiguration(ImageContext context, int defaultImage)
        {
            this.CombineFormula = new ImageFormula(defaultImage);
            this.AlphaFormula = new ImageFormula(defaultImage);

            parent = context;
            CombineFormula.Changed += (sender, args) => RecomputeImage = true;
            AlphaFormula.Changed += (sender, args) => RecomputeImage = true;
            parent.ChangedImages += (sender, args) =>
            {
                if (args.CurrentCount < args.PreviousCount)
                    RecomputeImage = true;
            };

            combineShader = new ImageCombineShader(context, CombineFormula, AlphaFormula);
        }

        /// <summary>
        /// returns the texture that shpuld be used for pixel displaying
        /// </summary>
        /// <returns></returns>
        public TextureArray2D GetStatisticsTexture()
        {
            // if the statistics texture is null, the display texture should be used. otherwise use the statistics texture
            var tex = DisplayTexture;
            if (statisticsTexture != null)
                tex = statisticsTexture;
            return tex;
        }

        /// <summary>
        /// binds the texture that should be used for pixel displaying
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="layer"></param>
        /// <param name="level">mipmap level</param>
        /// <returns></returns>
        public bool BindStatisticsTexture(int slot, int layer, int level)
        {
            if (DisplayTexture == null || !Active)
                return false;

            var tex = GetStatisticsTexture();

            // bind the final product
            tex.BindAsTexture2D(slot, layer, level);
            parent.BindSampler(slot, DisplayTexture.HasMipmaps(), false);
            return true;
        }

        /// <summary>
        /// updates data converning this image
        /// </summary>
        /// <return>true if image is ready to be drawn, false if image has to be processed</return>
        public bool Update()
        {
            if (!Active)
                return true;

            // update shader contents or make initial creation
            combineShader.Update();

            if (TonemappingStepable != null)
            {
                if (TonemappingStepable.HasStep())
                    TonemappingStepable.NextStep();

                if (!TonemappingStepable.HasStep())
                {
                    // finished stepping
                    TonemappingStepable = null;

                    // retrieve final picture
                    DisplayTexture = pingpong[0];
                    parent.TextureCache.StoreUnusuedTexture(pingpong[1]);
                    // retrieve the statistics texture if available
                    statisticsTexture = pingpong[2];
                    pingpong = null;
                    
                    return true;
                }
                return false;
            }

            if (RecomputeImage)
            {
                RecomputeImage = false;

                // dispose old statistics texture since it will probably be changed
                if (statisticsTexture != null)
                {
                    parent.TextureCache.StoreUnusuedTexture(statisticsTexture);
                    statisticsTexture = null;
                }

                // aqcuire texture if necessary
                if (DisplayTexture == null)
                    DisplayTexture = parent.TextureCache.GetAvailableTexture();

                RecomputeCombinedImage(DisplayTexture);

                if (UseTonemapper)
                {
                    // set up ping pong array
                    pingpong = new TextureArray2D[3];
                    pingpong[0] = DisplayTexture;
                    pingpong[1] = parent.TextureCache.GetAvailableTexture();
                    // this will be used for the statistics texture
                    pingpong[2] = null;

                    // create stepable
                    TonemappingStepable = parent.Tonemapper.GetApplyShaderStepable(pingpong,parent);

                    return false;
                }
            }

            return true;
        }

        private void RecomputeCombinedImage(TextureArray2D target)
        {
            if (parent.GetNumImages() == 0)
                return;

            for (int layer = 0; layer < parent.GetNumLayers(); ++layer)
            {
                for (int level = 0; level < parent.GetNumMipmaps(); ++level)
                {
                    for (int image = 0; image < parent.GetNumImages(); ++image)
                    {
                        parent.GetImageTexture(image).Bind(combineShader.GetSourceImageBinding(image));
                        parent.BindSampler(combineShader.GetSourceImageBinding(image), parent.GetImageTexture(image).HasMipmaps(), false);
                    }

                    combineShader.Run(layer, level, parent.GetWidth(level), parent.GetHeight(level), target);
                }
            }
        }

        public void AbortImageCalculation()
        {
            TonemappingStepable = null;
            if (pingpong != null)
            {
                DisplayTexture = pingpong[0];
                parent.TextureCache.StoreUnusuedTexture(pingpong[1]);
                pingpong = null;
            }
        }

        public void Dispose()
        {
            DisplayTexture?.Dispose();
            statisticsTexture?.Dispose();
            pingpong?[0]?.Dispose();
            pingpong?[1]?.Dispose();
            combineShader.Dispose();
        }
    }
}
