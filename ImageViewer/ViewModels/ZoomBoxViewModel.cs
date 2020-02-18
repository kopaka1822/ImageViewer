using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ImageFramework.Annotations;
using ImageFramework.Model.Overlay;
using ImageViewer.Models;
using ImageViewer.UtilityEx;

namespace ImageViewer.ViewModels
{
    public class ZoomBoxViewModel : INotifyPropertyChanged
    {
        public class BoxItem
        {
            public int Id { get; set; }
            public SolidColorBrush Brush { get; set; }
        }

        private readonly ModelsEx models;

        public ZoomBoxViewModel(ModelsEx models)
        {
            this.models = models;
            this.models.ZoomBox.Boxes.CollectionChanged += BoxesOnCollectionChanged;
            RefreshBoxes();
        }

        private void BoxesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshBoxes();
        }

        public List<BoxItem> Boxes { get; private set; }

        public bool HasBoxes => Boxes.Count != 0;

        private void RefreshBoxes()
        {
            var res = new List<BoxItem>();
            for (var i = 0; i < models.ZoomBox.Boxes.Count; i++)
            {
                var box = models.ZoomBox.Boxes[i];
                res.Add(new BoxItem
                {
                    Id = i,
                    Brush = box.Color.ToBrush()
                });
            }

            Boxes = res;
            OnPropertyChanged(nameof(Boxes));
            OnPropertyChanged(nameof(HasBoxes));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
