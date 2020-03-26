using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageViewer.Models.Settings;

namespace ImageViewer.ViewModels.Dialog
{
    public class ExportConfigViewModel : INotifyPropertyChanged
    {
        private static readonly bool defaultUse = false;

        private bool useImages = defaultUse;

        public bool UseImages
        {
            get => useImages;
            set
            {
                if (value == useImages) return;
                useImages = value;
                OnPropertyChanged(nameof(UseImages));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private bool addToImages = false;

        public bool AddToImages
        {
            get => addToImages;
            set
            {
                if (value == addToImages) return;
                addToImages = value;
                OnPropertyChanged(nameof(AddToImages));
            }
        }

        private bool useEquation = defaultUse;

        public bool UseEquation
        {
            get => useEquation;
            set
            {
                if (value == useEquation) return;
                useEquation = value;
                OnPropertyChanged(nameof(UseEquation));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private bool useFilter = defaultUse;

        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        private bool addToFilter = true;

        public bool AddToFilter
        {
            get => addToFilter;
            set
            {
                if (value == addToFilter) return;
                addToFilter = value;
                OnPropertyChanged(nameof(AddToFilter));
            }
        }

        private bool useExport = defaultUse;

        public bool UseExport
        {
            get => useExport;
            set
            {
                if (value == useExport) return;
                useExport = value;
                OnPropertyChanged(nameof(UseExport));
                OnPropertyChanged(nameof(IsValid));
            }
        }


        private bool useDisplay = defaultUse;

        public bool UseDisplay
        {
            get => useDisplay;
            set
            {
                if (value == useDisplay) return;
                useDisplay = value;
                OnPropertyChanged(nameof(UseDisplay));
                OnPropertyChanged(nameof(IsValid));
            }
        }

        public bool IsValid => UseImages || UseEquation || UseFilter || UseExport || UseDisplay;

        public ViewerConfig.Components UsedComponents
        {
            get
            {
                var c = ViewerConfig.Components.None;
                if (UseImages) c |= ViewerConfig.Components.Images;
                if (UseEquation) c |= ViewerConfig.Components.Equations;
                if (UseFilter) c |= ViewerConfig.Components.Filter;
                if (UseExport) c |= ViewerConfig.Components.Export;
                if (UseDisplay) c |= ViewerConfig.Components.Display;

                return c;
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
