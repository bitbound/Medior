using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Medior.Converters;

public class ValueEqualsParameterVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null && parameter is null)
        {
            return Visibility.Visible;
        }

        if (value is null || parameter is null)
        {
            return Visibility.Collapsed;
        }

        return value.Equals(parameter) ?
           Visibility.Visible :
           Visibility.Collapsed;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
