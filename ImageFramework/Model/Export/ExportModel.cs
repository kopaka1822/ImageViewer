using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;

namespace ImageFramework.Model.Export
{
    public class ExportModel : INotifyPropertyChanged
    {
        public IReadOnlyList<ExportFormatModel> Formats { get; }

        public ExportModel()
        {
            var formats = new List<ExportFormatModel>();
            formats.Add(new ExportFormatModel("png"));
            formats.Add(new ExportFormatModel("jpg"));
            formats.Add(new ExportFormatModel("bmp"));
            formats.Add(new ExportFormatModel("hdr"));
            formats.Add(new ExportFormatModel("pfm"));
            formats.Add(new ExportFormatModel("dds"));
            formats.Add(new ExportFormatModel("ktx"));
            Formats = formats;
        }

        private bool useCropping = false;

        public bool UseCropping
        {
            get => useCropping;
            set
            {
                if(value == useCropping) return;
                useCropping = value;
                OnPropertyChanged(nameof(UseCropping));
            }
        }

        public void Export(TextureArray2D image, ExportDescription desc)
        {
            Debug.Assert(image != null);

            // make image compatible with staging format
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
