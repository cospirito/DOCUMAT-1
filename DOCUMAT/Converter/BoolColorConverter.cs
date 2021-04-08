using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DOCUMAT.Converter
{
    public class BoolColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush(Colors.White);

            //if (value == null)
            //{
            //    return brush;
            //}

            //if ((bool)value)
            //{
            //    return new SolidColorBrush(Colors.White);
            //}
            //else
            //{
            //    return new SolidColorBrush(Colors.Red);
            //}

            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
