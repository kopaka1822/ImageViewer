using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using SharpDX.DXGI;

namespace ImageFramework.Model
{
    /// <summary>
    /// collection of imported images that will be used in image equations
    /// </summary>
    public class ImagesModel : INotifyPropertyChanged, IDisposable
    { 
        private struct Dimension
        {
            public int Width { get; set; }
            public int Height { get; set; }
        }

        private Dimension[] dimensions = null;

        public class MipmapMismatch : Exception
        {
            public MipmapMismatch(string message) : base(message)
            {}
        }

        public class ImageData : IDisposable
        {
            public TextureArray2D Image { get; private set; }
            public int NumMipmaps => Image.NumMipmaps;
            public int NumLayers => Image.NumLayers;
            public bool IsHdr => Image.Format == Format.R32G32B32A32_Float;
            public string Filename { get; }
            public GliFormat OriginalFormat { get; }

            internal ImageData(TextureArray2D image, string filename, GliFormat originalFormat)
            {
                Image = image;
                Filename = filename;
                OriginalFormat = originalFormat;
            }

            internal void GenerateMipmaps(int levels)
            {
                var tmp = Image.GenerateMipmapLevels(levels);
                Image.Dispose();
                Image = tmp;
            }

            internal void DeleteMipmaps()
            {
                var tmp = Image.CloneWithoutMipmaps();
                Image.Dispose();
                Image = tmp;
            }


            public void Dispose()
            {
                Image?.Dispose();
            }
        }

        private readonly List<ImageData> images = new List<ImageData>();

        #region Public Properties

        public IReadOnlyList<ImageData> Images { get; }

        // this property change will be triggered if the image order changes (and not if the number of images changes)
        public static string ImageOrder = nameof(ImageOrder);
        public int NumImages => images.Count;
        public int NumMipmaps => images.Count == 0 ? 0 : images[0].NumMipmaps;
        public int NumLayers => images.Count == 0 ? 0 : images[0].NumLayers;

        /// <summary>
        /// true if any image is hdr
        /// </summary>
        public bool IsHdr => images.Any(imageData => imageData.IsHdr);

        /// <summary>
        /// width for the biggest mipmap (or 0 if no images are present)
        /// </summary>
        public int Width => images.Count != 0 ? GetWidth(0) : 0;
        /// <summary>
        /// height for the biggest mipmap (or 0 if no images are present)
        /// </summary>
        public int Height => images.Count != 0 ? GetHeight(0) : 0;

        // helper for many other models
        /// <summary>
        /// previous number of images
        /// </summary>
        public int PrevNumImages { get; private set; } = 0;

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

        #endregion

        public ImagesModel()
        {
            Images = images;
        }

        /// <summary>
        /// tries to add the image to the current collection.
        /// </summary>
        /// <exception cref="MipmapMismatch">will be thrown if everything except the number of mipmaps matches</exception>
        /// <exception cref="Exception">will be thrown if the images does not match with the current set of images</exception>
        public void AddImage(TextureArray2D image, string name, GliFormat originalFormat)
        {
            if (images.Count == 0) // first image
            {
                InitDimensions(image);
                images.Add(new ImageData(image, name, originalFormat));
                PrevNumImages = 0;
                OnPropertyChanged(nameof(NumImages));
                OnPropertyChanged(nameof(NumLayers));
                OnPropertyChanged(nameof(NumMipmaps));
                OnPropertyChanged(nameof(IsHdr));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
            }
            else // test if compatible with previous images
            {
                if(image.NumLayers != NumLayers)
                    throw new Exception($"{name}: Inconsistent amount of layers. Expected {NumLayers} got {image.NumLayers}");

                if (image.GetWidth(0) != GetWidth(0) || image.GetHeight(0) != GetHeight(0))
                    throw new Exception($"{name}: Image resolution mismatch. Expected {GetWidth(0)}x{GetHeight(0)} but got {image.GetWidth(0)}x{image.GetHeight(0)}");

                if (image.NumMipmaps != NumMipmaps)
                    throw new MipmapMismatch($"{name}: Inconsistent amount of mipmaps. Expected {NumMipmaps} got {image.NumMipmaps}");

                // remember old properties
                var isHdr = IsHdr;

                images.Add(new ImageData(image, name, originalFormat));
                PrevNumImages = NumImages - 1;
                OnPropertyChanged(nameof(NumImages));

                if (isHdr != IsHdr)
                    OnPropertyChanged(nameof(IsHdr));
            }
        }

        public void DeleteImage(int imageId)
        {
            Debug.Assert(imageId >= 0 && imageId < NumImages);

            // remember old properties
            var isHdr = IsHdr;

            // delete old data
            images[imageId].Dispose();
            images.RemoveAt(imageId);

            PrevNumImages = NumImages + 1;
            OnPropertyChanged(nameof(NumImages));

            if (isHdr != IsHdr)
                OnPropertyChanged(nameof(IsHdr));

            if (NumImages == 0)
            {
                // everything was resettet
                OnPropertyChanged(nameof(NumLayers));
                OnPropertyChanged(nameof(NumMipmaps));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(Height));
            }
        }

        /// <summary>
        /// moves the image from index 1 to index 2
        /// </summary>
        /// <param name="idx1">current image index</param>
        /// <param name="idx2">index after moving the image</param>
        public void MoveImage(int idx1, int idx2)
        {
            Debug.Assert(idx1 >= 0);
            Debug.Assert(idx2 >= 0);
            Debug.Assert(idx1 < NumImages);
            Debug.Assert(idx2 < NumImages);
            if (idx1 == idx2) return;

            var i = images[idx1];
            images.RemoveAt(idx1);
            images.Insert(idx2, i);

            OnPropertyChanged(nameof(ImageOrder));
        }

        /// <summary>
        /// generates mipmaps for all images
        /// </summary>
        public void GenerateMipmaps()
        {
            Debug.Assert(NumMipmaps == 1);

            // compute new mipmap levels
            var levels = ComputeMaxMipLevels();
            if (levels == NumMipmaps) return;

            foreach (var image in images)
            {
                image.GenerateMipmaps(levels);
            }

            // recalc dimensions array
            var w = Width;
            var h = Height;
            dimensions = new Dimension[levels];
            for (int i = 0; i < levels; ++i)
            {
                dimensions[i].Width = w;
                dimensions[i].Height = h;
                w = Math.Max(w / 2, 1);
                h = Math.Max(h / 2, 1);
            }

            OnPropertyChanged(nameof(NumMipmaps));
        }

        public void DeleteMipmaps()
        {
            Debug.Assert(NumMipmaps > 1);
            foreach (var image in images)
            {
                image.DeleteMipmaps();
            }
            // refresh dimensions array
            var w = Width;
            var h = Height;
            dimensions = new Dimension[1];
            dimensions[0].Width = w;
            dimensions[0].Height = h;

            OnPropertyChanged(nameof(NumMipmaps));
        }

        private void InitDimensions(TextureArray2D image)
        {
            dimensions = new Dimension[image.NumMipmaps];
            for (var i = 0; i < image.NumMipmaps; ++i)
            {
                dimensions[i] = new Dimension
                {
                    Width = image.GetWidth(i),
                    Height = image.GetHeight(i)
                };
            }
        }

        /// <summary>
        /// computes the maximum amount of mipmap levels for the specified width and height
        /// </summary>
        /// <returns></returns>
        public static int ComputeMaxMipLevels(int width, int height)
        {
            var resolution = Math.Max(width, height);
            var maxMip = 1;
            while ((resolution /= 2) > 0) ++maxMip;
            return maxMip;
        }

        /// <summary>
        /// computes the maximum amount of mipmap levels for the current width and height
        /// </summary>
        /// <returns></returns>
        private int ComputeMaxMipLevels()
        {
            return ComputeMaxMipLevels(Width, Height);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}
