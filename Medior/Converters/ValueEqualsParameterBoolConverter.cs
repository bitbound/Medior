using System;
using System.Globalization;
using System.Windows.Data;

namespace Medior.Converters
{
    public class ValueEqualsParameterBoolConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null && parameter is null)
            {
                return true;
            }

            if (value is null || parameter is null)
            {
                return false;
            }

            return value.Equals(parameter);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
