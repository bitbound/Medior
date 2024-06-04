using System.Windows.Media;

namespace Medior.Models.Messages;

public class ToastMessage
{
    public ToastMessage(string message, ToastType type)
    {
        Message = message;
        Type = type;
    }

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
