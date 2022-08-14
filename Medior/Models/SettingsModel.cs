using Medior.Models.PhotoSorter;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using System;

namespace Medior.Models
{
    public class SettingsModel
    {
        public ClipboardSaveDto[] ClipboardSaves { get; set; } = Array.Empty<ClipboardSaveDto>();
        public UploadedFile[] FileUploads { get; set; } = Array.Empty<UploadedFile>();
        public bool HandlePrintScreen { get; set; } = true;
        public bool IsNavPaneOpen { get; set; } = true;
        public string EncryptedPrivateKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string ServerUri { get; set; } = string.Empty;
        public SortJob[] SortJobs { get; set; } = Array.Empty<SortJob>();
        public bool StartAtLogon { get; set; } = true;
        public AppTheme Theme { get; set; } = AppTheme.Dark;
        public string Username { get; set; } = string.Empty;
    }
}
