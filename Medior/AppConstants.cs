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
        public static string LogsFolderPath
        {
            get
            {
#if DEBUG
                return Path.Combine(Path.GetTempPath(), "Medior_Debug");
#else
                return Path.Combine(Path.GetTempPath(), "Medior");
#endif
            }
        }
        public static string RecordingsDirectory => Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Medior", "Recordings")).FullName;
        public static string SettingsFilePath
        {
            get
            {
#if DEBUG
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior", "settings_debug.json");
#else
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior", "settings.json");
#endif
            }
        }
    }
}
