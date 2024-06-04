using Medior.Models.PhotoSorter;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;

namespace Medior.Models;

public class SettingsModel
{
    public ClipboardSaveDto[] ClipboardSaves { get; set; } = [];
    public string EncryptedPrivateKey { get; set; } = string.Empty;
    public UploadedFile[] FileUploads { get; set; } = [];
    public bool HandlePrintScreen { get; set; } = true;
    public bool IsFileEncryptionEnabled { get; set; }
    public bool IsNavPaneOpen { get; set; } = true;
    public string PublicKey { get; set; } = string.Empty;
    public string ServerUri { get; set; } = string.Empty;
    public SortJob[] SortJobs { get; set; } = [];
    public bool StartAtLogon { get; set; } = true;
    public AppTheme Theme { get; set; } = AppTheme.Dark;
    public string Username { get; set; } = string.Empty;
}
