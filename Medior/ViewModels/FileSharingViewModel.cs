using MahApps.Metro.Controls.Dialogs;
using Medior;
using Medior.Shared.Entities;
using Medior.Shared.Helpers;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class FileSharingViewModel
    {
        private readonly IFileApi _fileApi;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<FileSharingViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly IProcessService _processes;
        private readonly IWindowService _windowService;
        private readonly IDialogService _dialogs;
        private readonly IServerUriProvider _serverUri;
        private readonly ISettings _settings;
        public FileSharingViewModel(
            IFileApi fileApi,
            IFileSystem fileSystem,
            IMessenger messenger,
            IServerUriProvider serverUriProvider,
            ISettings settings,
            IProcessService processService,
            IWindowService windowService,
            IDialogService dialogs,
            ILogger<FileSharingViewModel> logger)
        {
            _fileApi = fileApi;
            _fileSystem = fileSystem;
            _messenger = messenger;
            _serverUri = serverUriProvider;
            _settings = settings;
            _processes = processService;
            _windowService = windowService;
            _dialogs = dialogs;
            _logger = logger;

            RefreshUploads();

            FileUploads.CollectionChanged += FileUploads_CollectionChanged;
        }

        public ObservableCollectionEx<UploadedFile> FileUploads { get; } = new();

        public async Task UploadFiles(string[] fileList)
        {
            using var disposable = new CallbackDisposable(() =>
            {
                _messenger.Send(new LoaderUpdateMessage()
                {
                    IsShown = false
                });
            });

            var successCount = 0;
            foreach (var file in fileList)
            {
                try
                {
                    _messenger.Send(new LoaderUpdateMessage()
                    {
                        IsShown = true,
                        Type = LoaderType.Message,
                        Text = $"Uploading {Path.GetFileName(file)}"
                    });

                    using var fs = _fileSystem.OpenFileStream(file, FileMode.Open, FileAccess.Read);
                    var result = await _fileApi.UploadFile(fs, Path.GetFileName(file));
                    if (!result.IsSuccess)
                    {
                        _messenger.SendToast($"Failed to upload {Path.GetFileName(file)}", ToastType.Error);
                        continue;
                    }

                    FileUploads.Add(result.Value!);
                    successCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while uploading file {name}.", file);
                }
            }

            _messenger.SendToast($"Uploaded {successCount} file(s)", ToastType.Success);
            OnPropertyChanged(nameof(FileUploads));
        }

        public void RefreshUploads()
        {
            foreach (var file in _settings.FileUploads)
            {
                if (!FileUploads.Any(x => x.Id == file.Id))
                {
                    FileUploads.Add(file);
                }
            }
            OnPropertyChanged(nameof(FileUploads));
        }

        [RelayCommand]
        public async Task Clear()
        {
            var result = await _dialogs.ShowMessageAsync(
                "Confirm Delete",
                "Are you sure you want to delete all uploads from the server?",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            foreach (var file in FileUploads)
            {
                await _fileApi.DeleteFile(file);
            }

            FileUploads.Clear();

            _messenger.SendToast("Files deleted from server", ToastType.Success);
        }

        [RelayCommand]
        public void GetQrCode(UploadedFile file)
        {
            var url = $"{_serverUri.ServerUri}/file-sharing/{file.Id}?accessToken={file.AccessTokenView}";
            _windowService.ShowQrCode(url, "Share Link");
        }

        [RelayCommand]
        private void CopyLink(UploadedFile file)
        {
            var url = $"{_serverUri.ServerUri}/file-sharing/{file.Id}?accessToken={file.AccessTokenView}";
            Clipboard.SetText(url);
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }

        private void FileUploads_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _settings.FileUploads = FileUploads.ToArray();
        }

        [RelayCommand]
        private void OpenLink(UploadedFile file)
        {
            var url = $"{_serverUri.ServerUri}/file-sharing/{file.Id}?accessToken={file.AccessTokenView}";
            var psi = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true
            };
            _processes.Start(psi);
        }

        [RelayCommand]
        private async Task DeleteFile(UploadedFile file)
        {
            var result = await _fileApi.DeleteFile(file);

            if (result.IsSuccess)
            {
                FileUploads.Remove(file);
                _messenger.SendToast("File deleted", ToastType.Success);
            }
            else
            {
                _messenger.SendToast("File delete failed", ToastType.Error);
            }
        }
    }
}
