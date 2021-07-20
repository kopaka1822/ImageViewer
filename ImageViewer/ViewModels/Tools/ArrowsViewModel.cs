using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Model.Overlay;
using ImageViewer.Models;
using System.Windows.Media;
using ImageViewer.UtilityEx;

namespace ImageViewer.ViewModels.Tools
{
    public class ArrowsViewModel : INotifyPropertyChanged
    {
        public class ArrowItem
        {
            public int Id { get; set; }
            public SolidColorBrush Brush { get; set; }
        }

        private readonly ModelsEx models;

        public ArrowsViewModel(ModelsEx models)
        {
            this.models = models;
            this.models.Arrows.Arrows.CollectionChanged += ArrowsOnCollectionChanged;
            RefreshArrows();
        }

        public List<ArrowItem> Arrows { get; private set; }

        public bool HasArrows => Arrows.Count != 0;

        private void ArrowsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshArrows();
        }

        private void RefreshArrows()
        {
            var res = new List<ArrowItem>();
            for (var i = 0; i < models.Arrows.Arrows.Count; i++)
            {
                var a = models.Arrows.Arrows[i];
                res.Add(new ArrowItem()
                {
                    Id = i,
                    Brush = a.Color.ToBrush()
                });
            }

            Arrows = res;
            OnPropertyChanged(nameof(Arrows));
            OnPropertyChanged(nameof(HasArrows));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
