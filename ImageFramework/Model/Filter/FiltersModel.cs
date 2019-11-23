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

namespace ImageFramework.Model.Filter
{
    public class FiltersModel : IDisposable, INotifyPropertyChanged
    {
        private List<FilterModel> filter = new List<FilterModel>();
        private readonly ImagesModel images;

        public class RetargetErrorEventArgs : EventArgs
        {
            public RetargetErrorEventArgs(string error)
            {
                Error = error;
            }

            public string Error { get; private set; }
        }

        public delegate void RetargetErrorHandler(object sender, RetargetErrorEventArgs e);

        public event RetargetErrorHandler RetargetError;

        public FiltersModel(ImagesModel images)
        {
            this.images = images;
            this.images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.ImageType):
                    if (images.ImageType == typeof(TextureArray2D))
                    {
                        RetargetFilter(FilterLoader.TargetType.Tex2D);
                    }
                    else if (images.ImageType == typeof(Texture3D))
                    {
                        RetargetFilter(FilterLoader.TargetType.Tex3D);
                    }
                    break;
            }
        }

        public IReadOnlyList<FilterModel> Filter => filter;

        public FilterLoader.TargetType CurrentTarget { get; private set; } = FilterLoader.TargetType.Tex2D; // assume filter target 2d texture in the beginning        

        public class ParameterChangeEventArgs : EventArgs
        {
            public ParameterChangeEventArgs(FilterModel model, string propertyName)
            {
                Model = model;
                PropertyName = propertyName;
            }

            public FilterModel Model { get; }
            public string PropertyName { get; }
        }

        public delegate void ParameterChangeEventHandler(object sender, ParameterChangeEventArgs args);

        public event ParameterChangeEventHandler ParameterChanged;

        /// <summary>
        /// indicates if the filter model is inside the active filter list
        /// </summary>
        public bool IsUsed(FilterModel model)
        {
            return filter.Any(f => ReferenceEquals(f, model));
        }

        /// <summary>
        /// replaces the old filters with the new list of filters
        /// </summary>
        /// <param name="newFilter"></param>
        public void SetFilter(List<FilterModel> newFilter)
        {
            DisposeUnusedFilter(newFilter, filter);
            foreach (var filterModel in newFilter)
            {
                Debug.Assert(filterModel.Target == CurrentTarget);

                // only set the callback if it is a new filter
                if(filter.All(f => !ReferenceEquals(f, filterModel)))
                    SetParameterChangeCallback(filterModel);
            }
            filter = newFilter;
            OnPropertyChanged(nameof(Filter));
        }

        /// <summary>
        /// adds a single filter to the list
        /// </summary>
        public void AddFilter(FilterModel model)
        {
            if (filter.Count == 0 && images.NumImages == 0)
            {
                CurrentTarget = model.Target; // set target based on filter
            }
            Debug.Assert(model.Target == CurrentTarget);

            filter.Add(model);
            SetParameterChangeCallback(model);
            OnPropertyChanged(nameof(Filter));
        }

        /// <summary>
        /// deletes a single filter
        /// </summary>
        public void DeleteFilter(int index)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < filter.Count);
            filter[index].Dispose();
            filter.RemoveAt(index);
            OnPropertyChanged(nameof(Filter));
        }

        private void RetargetFilter(FilterLoader.TargetType target)
        {
            if (target == CurrentTarget) return;
            CurrentTarget = target;
            if (Filter.Count == 0) return;

            string errors = "";
            var newFilter = new List<FilterModel>();
            foreach (var f in Filter)
            {
                try
                {
                    newFilter.Add(f.Retarget(CurrentTarget));
                }
                catch (Exception)
                {
                    errors += $"filter {f.Name} was removed during retargeting\n";
                }
            }

            Dispose();
            filter = newFilter;
            OnPropertyChanged(nameof(Filter));

            if (errors.Length != 0)
                OnRetargetError(errors);
        }

        public void Clear()
        {
            Dispose();
            OnPropertyChanged(nameof(Filter));
        }

        public void Dispose()
        {
            foreach (var filterModel in filter)
            {
                filterModel.Dispose();
            }
            filter.Clear();
        }

        private void SetParameterChangeCallback(FilterModel model)
        {
            foreach (var filterParameter in model.Parameters)
            {
                filterParameter.GetBase().PropertyChanged +=
                    (sender, e) => OnParameterChanged(model, filterParameter.GetBase().Name);
            }

            foreach (var texParameter in model.TextureParameters)
            {
                texParameter.PropertyChanged += (sender, e) => OnParameterChanged(model, texParameter.Name);
            }

            model.PropertyChanged += (sender, e) => OnParameterChanged(model, e.PropertyName);
        }

        public static void DisposeUnusedFilter(IReadOnlyList<FilterModel> newList, IReadOnlyList<FilterModel> oldList)
        {
            foreach (var old in oldList)
            {
                // delete if the filter is not used in new list
                if (newList.All(newFilter => !ReferenceEquals(newFilter, old)))
                    old.Dispose();
            }
        }

        private void OnParameterChanged(FilterModel model, string parameterName)
        {
            ParameterChanged?.Invoke(this, new ParameterChangeEventArgs(model, parameterName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnRetargetError(string error)
        {
            RetargetError?.Invoke(this, new RetargetErrorEventArgs(error));
        }
    }
}
