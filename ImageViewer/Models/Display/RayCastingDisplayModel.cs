using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Utility;

namespace ImageViewer.Models.Display
{
    public class RayCastingDisplayModel : IExtendedDisplayModel
    {
        private readonly ImageFramework.Model.Models models;
        private readonly DisplayModel display;

        public RayCastingDisplayModel(ImageFramework.Model.Models models, DisplayModel display)
        {
            this.models = models;
            this.display = display;
            
        }

        public void Dispose()
        {
            
        }

        

        private bool flatShading = false;

        public bool FlatShading
        {
            get => flatShading;
            set
            {
                if (value == flatShading) return;
                flatShading = value;
                OnPropertyChanged(nameof(FlatShading));
            }
        }

        private bool useCropping = false;

        public bool UseCropping
        {
            get => useCropping;
            set
            {
                if (value == useCropping) return;
                useCropping = value;
                OnPropertyChanged(nameof(UseCropping));
            }
        }

        private bool selfShadowing = false;

        public bool SelfShadowing
        {
            get => selfShadowing;
            set
            {
                if (value == selfShadowing) return;
                selfShadowing = value;
                OnPropertyChanged(nameof(SelfShadowing));
            }
        }

        private bool alphaIsCoverage = false;

        public bool AlphaIsCoverage
        {
            get => alphaIsCoverage;
            set
            {
                if (value == alphaIsCoverage) return;
                alphaIsCoverage = value;
                OnPropertyChanged(nameof(AlphaIsCoverage));
            }
        }

        private bool hideInternals = false;
        public bool HideInternals
        {
            get => hideInternals;
            set
            {
                if (value == hideInternals) return;
                hideInternals = value;
                OnPropertyChanged(nameof(HideInternals));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler ForceTexelRecompute;

        protected virtual void OnForceTexelRecompute()
        {
            ForceTexelRecompute?.Invoke(this, EventArgs.Empty);
        }
    }
}
