using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TextureViewer.Models.Filter;

namespace TextureViewer.Views
{
    public class FilterProperties : ObservableCollection<object>
    {
        public FilterProperties(FilterModel item)
        {
            var margin = new Thickness(0.0, 0.0, 0.0, 2.0);

            Add(new TextBlock
            {
                Text = item.Name,
                Margin = margin,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 18.0
            });

            if(item.Description.Length > 0)
                Add(new TextBlock
                {
                    Text = item.Description,
                    Margin = new Thickness(0.0, 0.0, 0.0, 10.0),
                    TextWrapping = TextWrapping.Wrap
                });

            // add all settings
            foreach (var para in item.Parameters)
            {
                Add(new TextBlock
                {
                    Text = para.GetBase().Name + ":",
                    Margin = margin,
                    TextWrapping = TextWrapping.Wrap
                });

                switch (para.GetParamterType())
                {
                    case ParameterType.Float:

                        break;
                    case ParameterType.Int:
                        break;
                    case ParameterType.Bool:
                        break;
                }
            }
        }
    }
}
