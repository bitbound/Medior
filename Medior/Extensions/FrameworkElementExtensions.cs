using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Medior.Extensions
{
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
    }
}
