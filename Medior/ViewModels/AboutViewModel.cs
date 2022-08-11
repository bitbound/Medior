using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class AboutViewModel
    {
        private readonly IFileApi _api;
        private readonly IMessenger _messenger;
        private readonly IProcessService _processService;
        private readonly IUpdateChecker _updateChecker;

        [ObservableProperty]
        private bool _isNewVersionAvailable;

        public AboutViewModel(
            IProcessService processService, 
            IUpdateChecker updateChecker, 
            IMessenger messenger,
            IFileApi api)
        {
            _processService = processService;
            _updateChecker = updateChecker;
            _messenger = messenger;
            _api = api;
        }

        public async Task RefreshUpdateStatus()
        {
            await _updateChecker.CheckForUpdates(false);
            IsNewVersionAvailable = _updateChecker.IsNewVersionAvailable;
        }

        public string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version}";

        [RelayCommand]
        private async Task DownloadUpdate()
        {
            var downloadResult = await _api.DownloadDesktopSetup();

            if (!downloadResult.IsSuccess)
            {
                _messenger.SendToast("Failed to download update", ToastType.Error);
                return;
            }

            var psi = new ProcessStartInfo()
            {
                FileName = downloadResult.Value!,
                UseShellExecute = true
            };
            _processService.Start(psi);
        }

        [RelayCommand]
        private void OpenLogsFolder()
        {
            var psi = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = AppConstants.LogsFolderPath
            };
            _processService.Start(psi);
        }
    }
}
