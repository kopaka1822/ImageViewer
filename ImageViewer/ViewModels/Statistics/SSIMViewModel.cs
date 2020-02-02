using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageViewer.Models;

namespace ImageViewer.ViewModels.Statistics
{
    public class SSIMViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public SSIMViewModel(ModelsEx models, int id)
        {
            this.models = models;
            this.id = id;
        }

        private int id;

        public int Id
        {
            get => id;
            set
            {
                if(id == value) return;
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        // text box properties
        public string Luminance { get; set; }
        public string Contrast { get; set; }
        public string Structure { get; set; }
        public string SSIM { get; set; }
        public string DSSIM { get; set; }

        public bool IsValid { get; private set; } = false;

        public void ImportLuminance()
        {
            throw new NotImplementedException();
        }

        public void ImportContrast()
        {
            throw new NotImplementedException();
        }

        public void ImportStructure()
        {
            throw new NotImplementedException();
        }

        public void ImportSSIM()
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
