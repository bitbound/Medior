namespace Medior.Models
{
    public class SettingsModel
    {
        public AppTheme Theme { get; set; } = AppTheme.Dark;
        public string ServerUri { get; set; } = string.Empty;
        public bool HandlePrintScreen { get; set; } = true;
        public bool StartAtLogon { get; set; } = true;
    }
}
