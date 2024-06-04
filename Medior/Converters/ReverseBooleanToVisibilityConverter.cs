﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Medior.Converters;

internal class ReverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ?
                Visibility.Collapsed :
                Visibility.Visible;
        }

        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
