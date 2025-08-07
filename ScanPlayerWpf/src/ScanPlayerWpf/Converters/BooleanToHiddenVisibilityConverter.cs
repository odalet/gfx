using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ScanPlayerWpf.Converters
{
    public sealed class BooleanToHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolean = false;
            if (value is bool b)
                boolean = b;
            else if (value is bool?)
                boolean = ((bool?)value).GetValueOrDefault();

            return boolean ? Visibility.Visible : (object)Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is Visibility visibility && visibility == Visibility.Visible;

    }
}
