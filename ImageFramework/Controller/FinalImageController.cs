using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model;

namespace ImageFramework.Controller
{
    /// <summary>
    /// controller that keeps the final image up to date if changes occured
    /// </summary>
    internal class FinalImageController : INotifyPropertyChanged
    {
        private readonly Models models;
        private readonly FinalImageModel finalImage;

        public FinalImageController(Models models, FinalImageModel finalImage)
        {
            this.models = models;
            this.finalImage = finalImage;
            models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (HasChanges) return;
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.NumImages):

                    break;
                case nameof(ImagesModel.NumMipmaps):

                    break;
            }
        }


        private bool hasChanges = true;

        // indicates if the texture has to be recalculated due to formula or filter changes
        public bool HasChanges
        {
            get => hasChanges;
            set
            {
                if (value == hasChanges) return;
                hasChanges = value;
                if (hasChanges)
                    finalImage.Reset(); // remove texture because it is invalid now

                OnPropertyChanged(nameof(HasChanges));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
