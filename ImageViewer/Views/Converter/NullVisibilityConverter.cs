using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ImageViewer.Views.Converter
{
    public class NullVisibilityConverter : IValueConverter
    {
        private readonly Visibility nullValue;

        public NullVisibilityConverter(Visibility nullValue)
        {
            this.nullValue = nullValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? nullValue : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullCollapsedConverter : NullVisibilityConverter
    {
        public NullCollapsedConverter() : base(Visibility.Collapsed)
        {}
    }

    public class NullHiddenConverter : NullVisibilityConverter
    {
        public NullHiddenConverter() : base(Visibility.Hidden)
        { }
    }
}
