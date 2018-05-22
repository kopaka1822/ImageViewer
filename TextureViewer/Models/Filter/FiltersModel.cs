using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Annotations;
using TextureViewer.Commands;

namespace TextureViewer.Models.Filter
{
    public class FilterEvent : EventArgs
    {
        public FilterEvent(FilterModel model, int index)
        {
            Model = model;
            Index = index;
        }

        public FilterModel Model { get; }
        public int Index { get; }
    }

    public delegate void FilterAddEvent(FiltersModel source, FilterEvent item);
    public delegate void FilterRemoveEvent(FiltersModel source, FilterEvent item);

    public class FiltersModel
    {
        public event FilterAddEvent AddedFilter;
        public event FilterRemoveEvent RemovedFilter;

        private readonly List<FilterModel> filter = new List<FilterModel>();
        public IReadOnlyList<FilterModel> Filter => filter;
        public int NumFilter => filter.Count;

        public void Add(FilterModel model)
        {
            filter.Add(model);
            OnAddedFilter(model, filter.Count - 1);
        }

        public void Remove(FilterModel model)
        {
            var index = filter.FindIndex(m => ReferenceEquals(m, model));
            filter.RemoveAt(index);
            OnRemovedFilter(model, index);
        }

        protected virtual void OnAddedFilter(FilterModel item, int index)
        {
            AddedFilter?.Invoke(this, new FilterEvent(item, index));
        }

        protected virtual void OnRemovedFilter(FilterModel item, int index)
        {
            RemovedFilter?.Invoke(this, new FilterEvent(item, index));
        }
    }
}
