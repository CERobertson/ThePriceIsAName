namespace ThePriceIsAName
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class SplitTransform : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value).ZeroOnNaN() / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value).ZeroOnNaN() * 2;
        }
    }

    public static class DoubleExtensions
    {
        public static double ZeroOnNaN(this double d)
        {
            if (double.IsNaN(d))
            {
                return 0.0;
            }
            return d;
        }
    }
}