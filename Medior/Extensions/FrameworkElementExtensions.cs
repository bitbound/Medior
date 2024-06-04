using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Medior.Extensions;

public static class FrameworkElementExtensions
{
    public static void LaunchUrl(this Hyperlink hyperlink)
    {
        var psi = new ProcessStartInfo()
        {
            FileName = hyperlink.NavigateUri.ToString(),
            UseShellExecute = true
        };
        Process.Start(psi);
    }

    public static void ShowTooltip(this FrameworkElement element, string message)
    {
        element.ToolTip = new ToolTip()
        {
            Content = message,
            IsOpen = true,
            StaysOpen = false
        };
    }
}
