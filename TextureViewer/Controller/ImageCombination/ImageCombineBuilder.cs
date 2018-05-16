using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.glhelper;
using TextureViewer.Models;

namespace TextureViewer.Controller.ImageCombination
{
    /// <summary>
    /// this is used to assist the process of creating a new image (with image combination and filter)
    /// </summary>
    public class ImageCombineBuilder
    {
        private TextureArray2D primary = null;
        private TextureArray2D temp = null;
        private TextureArray2D statistics = null;
        private bool primaryIsStatistics = false;
        private readonly TextureCacheModel textureCache;

        public ImageCombineBuilder(TextureCacheModel textureCache)
        {
            this.textureCache = textureCache;
        }

        /// <summary>
        /// this texture should contain the final result
        /// </summary>
        /// <returns>valid texture</returns>
        public TextureArray2D GetPrimaryTexture()
        {
            if (primary != null) return primary;
            primary = textureCache.GetTexture();
            return primary;
        }

        /// <summary>
        /// marks the current primary texture as statistics texture.
        /// </summary>
        public void UsePrimaryAsStatistics()
        {
            primaryIsStatistics = true;
        }

        /// <summary>
        /// this texture can be used for ping pong rendering
        /// </summary>
        /// <returns>valid texture</returns>
        public TextureArray2D GetTemporaryTexture()
        {
            if (temp != null) return temp;
            temp = textureCache.GetTexture();
            return temp;
        }

        /// <summary>
        /// swaps the primary and temporary textures.
        /// saves the primary texture as statistics texture if marked
        /// </summary>
        public void SwapPrimaryAndTemporary()
        {
            if (primaryIsStatistics)
            {
                // save primary (dont overdraw it)
                if(statistics != null)
                    textureCache.StoreTexture(statistics);

                statistics = primary;
                primary = null;
                primaryIsStatistics = false;
            }

            // swap
            var t = primary;
            primary = temp;
            temp = t;
        }

        /// <summary>
        /// returns the current statistics texture (or the primary texture if nothing was set)
        /// </summary>
        public TextureArray2D GetStatisticsTexture()
        {
            return statistics ?? primary;
        }

        /// <summary>
        /// clears the temporary texture
        /// </summary>
        public void Dispose()
        {
            if (temp != null)
            {
                textureCache.StoreTexture(temp);
                temp = null;
            }
        }
    }
}
