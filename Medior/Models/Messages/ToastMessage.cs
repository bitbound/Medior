using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Medior.Models.Messages
{
    public class ToastMessage
    {

        public string Message { get; init; } = string.Empty;
        public ToastType Type { get; init; } = ToastType.Information;

        public Brush BackgroundColor =>
            Type switch
            {
                ToastType.Information => new SolidColorBrush(Colors.DimGray),
                ToastType.Success => new SolidColorBrush(Colors.ForestGreen),
                ToastType.Warning => new SolidColorBrush(Colors.DarkOrange),
                ToastType.Error => new SolidColorBrush(Colors.DarkRed),
                _ => new SolidColorBrush(Colors.DimGray)
            };
    }

    public enum ToastType
    {
        Information,
        Success,
        Warning,
        Error
    }
}
