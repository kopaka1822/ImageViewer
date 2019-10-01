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
using ImageFramework.Model.Equation;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    public class FinalImageModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Models models;
        public ImageEquationModel Equation { get; }

        public FinalImageModel(Models models, int defaultImage = 0)
        {
            this.models = models;
            Equation = new ImageEquationModel(models.Images, defaultImage);
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
            Equation.PropertyChanged += EquationOnPropertyChanged;
            Equation.Color.PropertyChanged += FormulaOnPropertyChanged;
            Equation.Alpha.PropertyChanged += FormulaOnPropertyChanged;
        }

        private void FormulaOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FormulaModel.Converted):
                    HasChanges = true;
                    break;
            }
        }

        private void EquationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImageEquationModel.IsValid):
                    HasChanges = true; // valid status changed => changes on image count have occurred
                    OnPropertyChanged(nameof(IsValid));
                    break;
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if(models.Images.NumImages == 0)
                        Reset();

                    break;
                case nameof(ImagesModel.NumMipmaps):
                    Reset();
                    break;
            }
        }

        /// <summary>
        /// this is the non-tonemapped result
        /// </summary>
        public TextureArray2D Texture { get; private set; }

        private bool hasChanges = true;

        // indicates if the texture has to be recalculated due to formula or filter changes
        public bool HasChanges
        {
            get => hasChanges;
            set
            {
                if (value == hasChanges) return;
                hasChanges = value;
                if(hasChanges)
                    Reset(); // remove texture because it is invalid now

                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public bool IsValid => Equation.IsValid;

        private bool useFilter = true;
        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if(value == useFilter) return;
                useFilter = value;
                // TODO only trigger if filter number > 0
                HasChanges = true;
                OnPropertyChanged(nameof(UseFilter));
            }
        }

        /// <summary>
        /// should be called if HasChanges is true to recalculate the texture
        /// </summary>
        public void Update()
        {
            Debug.Assert(HasChanges);

            Reset();
            Texture = models.TexCache.GetTexture();

            using (var combineShader = new ImageCombineShader(Equation.Color.Converted, Equation.Alpha.Converted,
                models.Images.NumImages))
            {
                combineShader.Run(models.Images, models.DxModel.LayerLevelBuffer, Texture);
            }

            OnPropertyChanged(nameof(Texture));
            HasChanges = false;
        }

        private void Reset()
        {
            if (Texture != null)
            {
                models.TexCache.StoreTexture(Texture);
                Texture = null;
            }
        }

        public void Dispose()
        {
            Texture?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
