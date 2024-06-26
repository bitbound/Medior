﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Medior.Converters;

public class DateTimeOffsetToLocalTimeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTimeOffset offset)
        {
            return offset.LocalDateTime;
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
