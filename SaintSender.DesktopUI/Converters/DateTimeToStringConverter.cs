using System;
using System.Globalization;
using System.Windows.Data;

namespace SaintSender.DesktopUI.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        private const string Format = "dddd, dd MMM yyyy h:mm";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is DateTime d ? d.ToString(Format) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DateTime.ParseExact(value?.ToString(), Format, culture);
        }
    }
}
