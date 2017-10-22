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
        // texture after combinding before tonemapping
        private TextureArray2D combinedTexture;
        // texture after tonemapping
        public TextureArray2D Texture { get; private set; }
        public ImageFormula CombineFormula { get; } = new ImageFormula();
        public ImageFormula AlphaFormula { get; } = new ImageFormula();
        public bool Active { get; set; } = true;

        public ImageConfiguration(ImageContext context)
        {
            parent = context;
            CombineFormula.Changed += (sender, args) => RecomputeImage = true;
            AlphaFormula.Changed += (sender, args) => RecomputeImage = true;
            
            combineShader = new ImageCombineShader(context, CombineFormula, AlphaFormula);
        }

        /// <summary>
        /// binds the texture that should be used for pixel displaying
        /// (depending on DisplayColorBeforeTonemapping)
        /// </summary>
        /// <param name="slot"></param>
        /// <param name="layer"></param>
        /// <param name="level">mipmap level</param>
        /// <returns></returns>
        public bool BindPixelDisplayTextue(int slot, int layer, int level)
        {
            if (Texture == null || !Active)
                return false;

            if (parent.DisplayColorBeforeTonemapping)
            {
                if (UseTonemapper && parent.Tonemapper.HasTonemapper())
                {
                    // since tonemappers were used the final texture differs from the combined
                    if (combinedTexture == null)
                    {
                        // recalculate the combined texture
                        combinedTexture = parent.TextureCache.GetAvailableTexture();
                        RecomputeCombinedImage(combinedTexture);
                    }

                    combinedTexture.BindAsTexture2D(slot, false, layer, level);
                    return true;
                }
                // just use the final texture since no tonemappers were used
            }

            if (combinedTexture != null)
            {
                // we can get rid of this texture since we use the final one for displaying now
                parent.TextureCache.StoreUnusuedTexture(combinedTexture);
                combinedTexture = null;
            }

            // bind the final product
            Texture.BindAsTexture2D(slot, false, layer, level);

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
            
            if (RecomputeImage)
            {
                RecomputeImage = false;
                // put the last used combined texture back in the cache (will probably be changed)
                if (combinedTexture != null)
                {
                    parent.TextureCache.StoreUnusuedTexture(combinedTexture);
                    combinedTexture = null;
                }

                // aqcuire texture if necessary
                if (Texture == null)
                    Texture = parent.TextureCache.GetAvailableTexture();                

                RecomputeCombinedImage(Texture);

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

        public void Dispose()
        {
            Texture?.Dispose();
            combinedTexture?.Dispose();
            pingpong?[0]?.Dispose();
            pingpong?[1]?.Dispose();
            combineShader.Dispose();
        }
    }
}
