﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SaintSender.DesktopUI.Converters
{
    public class BooleanToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b ? FontWeights.Normal : FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is FontWeight f && f == FontWeights.Normal;
        }
    }
}
