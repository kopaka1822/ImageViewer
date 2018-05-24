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
    public class FiltersModel
    {
        private List<FilterModel> filter = new List<FilterModel>();
        public IReadOnlyList<FilterModel> Filter => filter;
        public int NumFilter => filter.Count;

        /// <summary>
        /// returns if the shader is in use by the active model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsUsed(FilterModel model)
        {
            return filter.Any(f => ReferenceEquals(f, model));
        }

        public void Apply(List<FilterModel> models, OpenGlContext context)
        {
            DisposeUnusedFilter(models, filter, context);
            filter = models;
        }

        /// <summary>
        /// disposes all filters from old list which are no longer used in new list
        /// </summary>
        /// <param name="newList"></param>
        /// <param name="oldList"></param>
        /// <param name="context"></param>
        public static void DisposeUnusedFilter(IReadOnlyList<FilterModel> newList, IReadOnlyList<FilterModel> oldList, OpenGlContext context)
        {
            var disable = context.Enable();

            foreach (var old in oldList)
            {
                // delete if the filter is not used in new list
                if(newList.All(newFilter => !ReferenceEquals(newFilter, old)))
                    old.Dispose();
            }

            if(disable)
                context.Disable();
        }
    }
}
