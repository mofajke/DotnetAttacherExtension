using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotnetAttacher.Window.Converter
{
    public class AlreadyAttachedToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var alreadyAttachedId = (int)value[0];
                var selectedIndex = (int)value[1];

                return alreadyAttachedId <= 0 && selectedIndex >= 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
