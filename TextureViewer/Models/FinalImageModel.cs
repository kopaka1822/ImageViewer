using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.glhelper;

namespace TextureViewer.Models
{
    public class FinalImageModel : INotifyPropertyChanged
    {
        private readonly TextureCacheModel textureCache;
        private readonly ImagesModel images;

        public FinalImageModel(TextureCacheModel textureCache, ImagesModel images)
        {
            this.textureCache = textureCache;
            this.images = images;
            this.images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if (images.NumImages == 0)
                    {
                        // dispose all textures
                        Reset();
                    }
                    break;
            }
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

            OnPropertyChanged(nameof(Texture));
            OnPropertyChanged(nameof(StatisticsTexture));
        }

        public void Dispose()
        {
            Texture?.Dispose();
            statisticsTexture?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
