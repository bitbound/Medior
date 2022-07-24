using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior
{
    internal static class AppConstants
    {
        public static string LogsFolderPath = Path.Combine(Path.GetTempPath(), "Medior");
        public static string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior", "settings.json");
    }
}
