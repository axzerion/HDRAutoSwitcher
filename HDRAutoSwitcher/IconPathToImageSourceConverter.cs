using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HDRAutoSwitcher
{
    internal class IconPathToImageSourceConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    ImageSource? src = IconService.GetIcon(path);

                    if (src != null)
                        return src;
                }
                catch
                {
                    // fall through to null
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
