using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TextureViewer.Annotations;
using TextureViewer.Controller;
using TextureViewer.glhelper;

namespace TextureViewer.Models
{
    public class ImagesModel: INotifyPropertyChanged
    {
        private OpenGlController glController;

        private struct Dimension
        {
            public int Width;
            public int Height;
        }

        private Dimension[] dimensions = null;

        private class ImageData
        {
            public TextureArray2D TextureArray { get; }
            public int NumMipmaps { get; }
            public int NumLayers { get; }
            public bool IsGrayscale { get; }
            public bool HasAlpha { get; }
            public bool IsHdr { get; }
            public string Filename { get; }

            public ImageData(ImageLoader.Image image)
            {
                this.TextureArray = new TextureArray2D(image);
                NumMipmaps = image.NumMipmaps;
                NumLayers = image.NumLayers;
                IsGrayscale = image.IsGrayscale();
                HasAlpha = image.HasAlpha();
                IsHdr = image.IsHdr();
                Filename = image.Filename;
            }

            /// <summary>
            /// disposes all opengl related data.
            /// Component should not be used after this
            /// </summary>
            public void Dispose()
            {
                TextureArray.Dispose();
            }
        }

        private readonly List<ImageData> images;

        public ImagesModel(OpenGlController glController)
        {
            this.glController = glController;
            images = new List<ImageData>();
        }

        #region Public Members

        public int NumImages => images.Count;
        public int NumMipmaps => images.Count == 0 ? 0 : images[0].NumMipmaps;
        public int NumLayers => images.Count == 0 ? 0 : images[0].NumLayers;
        /// <summary>
        /// true if all images are grayscale
        /// </summary>
        public bool IsGrayscale => images.All(imageData => imageData.IsGrayscale);
        /// <summary>
        /// true if any image has an alpha channel
        /// </summary>
        public bool IsAlpha => images.Any(imageData => imageData.HasAlpha);
        /// <summary>
        /// true if any image is hdr
        /// </summary>
        public bool IsHdr => images.Any(imageData => imageData.IsHdr);

        /// <summary>
        /// width of the mipmap
        /// </summary>
        /// <param name="mipmap"></param>
        /// <returns></returns>
        public int GetWidth(int mipmap)
        {
            Debug.Assert(images.Count != 0);
            Debug.Assert(mipmap < NumMipmaps && mipmap >= 0);
            return dimensions[mipmap].Width;
        }

        /// <summary>
        /// height of the mipmap
        /// </summary>
        /// <param name="mipmap"></param>
        /// <returns></returns>
        public int GetHeight(int mipmap)
        {
            Debug.Assert(images.Count != 0);
            Debug.Assert(mipmap < NumMipmaps && mipmap >= 0);
            return dimensions[mipmap].Height;
        }

        public string GetFilename(int image)
        {
            Debug.Assert((uint)(image) < images.Count);
            return images[image].Filename;
        }

        public TextureArray2D GetTexture(int image)
        {
            Debug.Assert((uint)(image) < images.Count);
            return images[image].TextureArray;
        }

        /// <summary>
        /// tries to add the image to the current collection.
        /// Throws an exception if the image cannot be added
        /// </summary>
        /// <param name="imgs">images that should be added</param>
        public void AddImages(List<ImageLoader.Image> imgs)
        {
            Debug.Assert(!glController.IsEnabled);
            glController.Enable();
            foreach (var image in imgs)
            {
                if (images.Count == 0)
                {
                    images.Add(new ImageData(image));

                    // intialize dimensions
                    dimensions = new Dimension[image.NumMipmaps];
                    for (var i = 0; i < image.NumMipmaps; ++i)
                    {
                        dimensions[i] = new Dimension()
                        {
                            Height = image.GetHeight(i),
                            Width = image.GetWidth(i)
                        };
                    }

                    // a lot has changed
                    OnPropertyChanged(nameof(NumImages));
                    OnPropertyChanged(nameof(NumLayers));
                    OnPropertyChanged(nameof(NumMipmaps));
                    OnPropertyChanged(nameof(IsAlpha));
                    OnPropertyChanged(nameof(IsGrayscale));
                    OnPropertyChanged(nameof(IsHdr));
                }
                else // test if image compatible with previous images
                {
                    if(image.Layers.Count != NumLayers)
                        throw new Exception($"Inconsistent amount of layers. Expected {NumLayers} got {image.Layers.Count}");

                    if (image.NumMipmaps != NumMipmaps)
                        throw new Exception($"Inconsistent amount of mipmaps. Expected {NumMipmaps} got {image.NumMipmaps}");

                    // remember old properties
                    var isAlpha = IsAlpha;
                    var isGrayscale = IsGrayscale;
                    var isHdr = IsHdr;

                    // test mipmaps
                    for (var level = 0; level < NumMipmaps; ++level)
                    {
                        if (image.GetWidth(level) != GetWidth(level) || image.GetHeight(level) != GetHeight(level))
                            throw new Exception(
                                $"Inconsistent mipmaps dimension. Expected {GetWidth(level)}x{GetHeight(level)}" +
                                $" got {image.GetWidth(level)}x{image.GetHeight(level)}");
                    }

                    images.Add(new ImageData(image));

                    OnPropertyChanged(nameof(NumImages));
                    if(isAlpha != IsAlpha)
                        OnPropertyChanged(nameof(isAlpha));
                    if(isGrayscale != IsGrayscale)
                        OnPropertyChanged(nameof(IsGrayscale));
                    if(isHdr != IsHdr)
                        OnPropertyChanged(nameof(IsHdr));
                }
            }
            glController.Disable();
        }

        /// <summary>
        /// deletes an image including all opengl data
        /// </summary>
        /// <param name="imageId"></param>
        public void DeleteImage(int imageId)
        {
            Debug.Assert(imageId >= 0 && imageId < NumImages);

            // remember old properties
            var isAlpha = IsAlpha;
            var isGrayscale = IsGrayscale;
            var isHdr = IsHdr;

            Debug.Assert(!glController.IsEnabled);
            glController.Enable();
            // delete old data
            images[imageId].Dispose();
            glController.Disable();

            images.RemoveAt(imageId);
            OnPropertyChanged(nameof(NumImages));

            if (isAlpha != IsAlpha)
                OnPropertyChanged(nameof(isAlpha));
            if (isGrayscale != IsGrayscale)
                OnPropertyChanged(nameof(IsGrayscale));
            if (isHdr != IsHdr)
                OnPropertyChanged(nameof(IsHdr));

            if (NumImages == 0)
            {
                // everything was resettet
                OnPropertyChanged(nameof(NumLayers));
                OnPropertyChanged(nameof(NumMipmaps));
            }
        }

        /// <summary>
        /// deletes all opengl related data.
        /// Component should not be used after this
        /// </summary>
        public void Dispose()
        {
            foreach (var imageData in images)
            {
                imageData.Dispose();
            }
            images.Clear();
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
