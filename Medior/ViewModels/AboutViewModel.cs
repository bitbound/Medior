using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class AboutViewModel
    {
        private readonly IApiService _api;
        private readonly IMessenger _messenger;
        private readonly IProcessService _processService;

        [ObservableProperty]
        private string _downloadUrl;

        [ObservableProperty]
        private bool _isNewVersionAvailable;

        public AboutViewModel(
            IProcessService processService, 
            IUpdateChecker updateChecker, 
            IMessenger messenger,
            IApiService api,
            IServerUriProvider serverUriProvider)
        {
            _processService = processService;
            _isNewVersionAvailable = updateChecker.IsNewVersionAvailable;
            _messenger = messenger;
            _api = api;
            _downloadUrl = $"{serverUriProvider.ServerUri}/downloads/MediorSetup.exe";
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
