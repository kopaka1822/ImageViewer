using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.Models.Filter;
using TextureViewer.Views;

namespace TextureViewer.ViewModels
{
    public class FiltersViewModel : INotifyPropertyChanged
    {
        private Models.Models models;

        public FiltersViewModel(Models.Models models)
        {
            this.models = models;
            models.Filter.AddedFilter += FilterOnAddedFilter;
            models.Filter.RemovedFilter += FilterOnRemovedFilter;
        }

        private void FilterOnRemovedFilter(FiltersModel source, FilterEvent item)
        {
            AvailableFilter.RemoveAt(item.Index);
            OnPropertyChanged(nameof(AvailableFilter));
        }

        private void FilterOnAddedFilter(FiltersModel source, FilterEvent item)
        {
            AvailableFilter.Insert(item.Index, new FilterListBoxItem(source, item.Model));
            OnPropertyChanged(nameof(AvailableFilter));
        }

        public ObservableCollection<FilterListBoxItem> AvailableFilter { get; } = new ObservableCollection<FilterListBoxItem>();

        private FilterListBoxItem selectedFilter = null;

        public FilterListBoxItem SelectedFilter
        {
            get => selectedFilter;
            set
            {
                if (Equals(selectedFilter, value)) return;
                selectedFilter = value;
                OnPropertyChanged(nameof(SelectedFilter));
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
