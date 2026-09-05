using System;
using System.Globalization;
using System.Windows.Data;

namespace HDRAutoSwitcher
{
    internal class BoolToHdrConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? "HDR" : "SDR";
            }

            return "SDR";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
