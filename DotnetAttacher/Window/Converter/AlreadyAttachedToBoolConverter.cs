using System;
using System.Globalization;
using System.Windows.Data;

namespace DotnetAttacher.Window.Converter
{
    public class AlreadyAttachedToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var v = (int) value;
                return v <= 0;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
