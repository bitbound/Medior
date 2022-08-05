using System;
using System.IO;

namespace Medior
{
    internal static class AppConstants
    {
        public static string LogsFolderPath
        {
            get
            {
                var tempDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Medior")).FullName;
#if DEBUG
                return Path.Combine(tempDir, "Logs_Debug");
#else
                return Path.Combine(tempDir, "Logs");
#endif
            }
        }

        public static string PhotoSorterReportsDir
        {
            get
            {
                return Directory.CreateDirectory(Path.Combine(LogsFolderPath, "PhotoSorter")).FullName;
            }
        }

        public static string ImagesDirectory => Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Medior", "Images")).FullName;
        public static string RecordingsDirectory => Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Medior", "Recordings")).FullName;
        public static string SettingsFilePath
        {
            get
            {
                var appDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior");
                Directory.CreateDirectory(appDir);
#if DEBUG
                return Path.Combine(appDir, "settings_debug.json");
#else
                return Path.Combine(appDir, "settings.json");
#endif
            }
        }
    }
}
