using System;
using System.Globalization;
using System.Windows.Data;

namespace SaintSender.DesktopUI.Converters
{
    public class DateTimeConverterForEmailDate : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime)value;
            var formattedDateTime = dateTime.ToString("dddd, dd MMM yyyy h:mm");
            return formattedDateTime;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateString = (string)value;
            var format = "dddd, dd MMM yyyy h:mm";
            return DateTime.ParseExact(dateString, format, culture);
        }
    }
}
