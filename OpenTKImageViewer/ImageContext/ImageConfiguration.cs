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
        public bool RecomputeCpuTexture { get; set; } = true;

        public bool UseTonemapper
        {
            get => useTonemapper;
            set {
                if (value == useTonemapper) return;
                useTonemapper = value;
                RecomputeImage = true;
            }
        }
        public IStepable TonemappingStepable { get; private set; }= null;
        public TextureArray2D Texture { get; private set; }
        public ImageFormula CombineFormula { get; } = new ImageFormula();
        public ImageFormula AlphaFormula { get; } = new ImageFormula();
        public bool IsEnabled { get; set; } = true;
        public CpuTexture CpuCachedTexture { get; private set; }
        public bool Active { get; set; } = true;

        public ImageConfiguration(ImageContext context)
        {
            parent = context;
            CombineFormula.Changed += (sender, args) => RecomputeImage = true;
            AlphaFormula.Changed += (sender, args) => RecomputeImage = true;
            
            combineShader = new ImageCombineShader(context, CombineFormula, AlphaFormula);
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

            // update cpu texture (from before tonemapping to after tonemapping)
            if (RecomputeCpuTexture && Texture != null)
            {
                CpuCachedTexture = Texture.GetFloatPixels(parent.GetNumMipmaps(), parent.GetNumLayers());
                RecomputeCpuTexture = false;
            }
            
            if (RecomputeImage)
            {
                RecomputeImage = false;
                if (Texture == null)
                    Texture = parent.TextureCache.GetAvailableTexture();

                RecomputeCombinedImage(Texture);

                // retrieve the complete image for the cpu
                if (parent.DisplayColorBeforeTonemapping || !UseTonemapper)
                    CpuCachedTexture = Texture.GetFloatPixels(parent.GetNumMipmaps(), parent.GetNumLayers());

                if (UseTonemapper)
                {
                    // set up ping pong array
                    pingpong = new TextureArray2D[2];
                    pingpong[0] = Texture;
                    pingpong[1] = parent.TextureCache.GetAvailableTexture();

                    // create stepable
                    TonemappingStepable = parent.Tonemapper.GetApplyShaderStepable(pingpong, parent);
                }
            }

            if (TonemappingStepable != null)
            {
                if (TonemappingStepable.HasStep())
                    TonemappingStepable.NextStep();

                if (!TonemappingStepable.HasStep())
                {
                    // finished stepping
                    TonemappingStepable = null;

                    // retrieve final picture
                    Texture = pingpong[0];
                    parent.TextureCache.StoreUnusuedTexture(pingpong[1]);
                    pingpong = null;

                    // retrieve the complete image for the cpu
                    if (!parent.DisplayColorBeforeTonemapping && Texture != null)
                        CpuCachedTexture = Texture.GetFloatPixels(parent.GetNumMipmaps(), parent.GetNumLayers());
                    return true;
                }
                return false;
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
                        parent.GetImageTexture(image).Bind(combineShader.GetSourceImageBinding(image), false);
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
                Texture = pingpong[0];
                parent.TextureCache.StoreUnusuedTexture(pingpong[1]);
                pingpong = null;
            }
        }
    }
}
