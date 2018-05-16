using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.glhelper;

namespace TextureViewer.Models
{
    public class FinalImageModel
    {
        private readonly TextureCacheModel textureCache;

        public FinalImageModel(TextureCacheModel textureCache)
        {
            this.textureCache = textureCache;
        }

        /// <summary>
        /// this texture should be used displaying
        /// </summary>
        public TextureArray2D Texture { get; private set; }

        /// <summary>
        /// this texture should be used for the pixel information in the status bar
        /// </summary>
        public TextureArray2D StatisticsTexture => statisticsTexture ?? Texture;
        private TextureArray2D statisticsTexture = null;

        /// <summary>
        /// resets the model to the initial state (empty textures)
        /// </summary>
        public void Reset()
        {
            if (Texture != null)
            {
                textureCache.StoreTexture(Texture);
                Texture = null;
            }

            if (statisticsTexture != null)
            {
                textureCache.StoreTexture(statisticsTexture);
                statisticsTexture = null;
            }
        }

        /// <summary>
        /// applies the images to the model.
        /// reset should be called before using this
        /// </summary>
        /// <param name="finalTexture">not null</param>
        /// <param name="statTexture">texture for image statistics. Not null, may be the same as final texture</param>
        public void Apply(TextureArray2D finalTexture, TextureArray2D statTexture)
        {
            Debug.Assert(finalTexture != null);
            Debug.Assert(statTexture != null);
            Debug.Assert(Texture == null);
            Debug.Assert(statisticsTexture == null);

            Texture = finalTexture;
            if (finalTexture != statTexture)
            {
                statisticsTexture = statTexture;
            }
        }

        public void Dispose()
        {
            Texture?.Dispose();
            statisticsTexture?.Dispose();
        }
    }
}
