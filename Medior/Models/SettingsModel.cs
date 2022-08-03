using Medior.Models.PhotoSorter;
using System;

namespace Medior.Models
{
    public class SettingsModel
    {
        public bool HandlePrintScreen { get; set; } = true;
        public bool IsNavPaneOpen { get; set; } = true;
        public string ServerUri { get; set; } = string.Empty;
        public bool StartAtLogon { get; set; } = true;
        public AppTheme Theme { get; set; } = AppTheme.Dark;
        public SortJob[] SortJobs { get; set; } = Array.Empty<SortJob>();
    }
}
