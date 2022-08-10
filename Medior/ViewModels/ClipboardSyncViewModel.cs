using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class ClipboardSyncViewModel
    {
        private readonly IDesktopHubConnection _hubConnection;
        private readonly IMessenger _messenger;
        private readonly ILogger<ClipboardSyncViewModel> _logger;

        [ObservableProperty]
        private ImageSource? _qrCodeImage;

        [ObservableProperty]
        private string _syncUrl = string.Empty;

        public ClipboardSyncViewModel(
            IMessenger messenger,
            IDesktopHubConnection hubConnection,
            ILogger<ClipboardSyncViewModel> logger)
        {
            _hubConnection = hubConnection;
            _messenger = messenger;
            _logger = logger;
        }

        private bool IsCopyEnabled => !string.IsNullOrWhiteSpace(SyncUrl);


        [RelayCommand(CanExecute = nameof(IsCopyEnabled))]
        private void CopySyncUrl()
        {
            Clipboard.SetText(_syncUrl);
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }

        [RelayCommand]
        private async Task Receive()
        {
            var url = await _hubConnection.GetClipboardReceiveUrl();
        }

        [RelayCommand]
        private async Task Send()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                var url = await _hubConnection.GetClipboardSendUrl();
            }
            else if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                var buffer = image.ToBuffer();
                var url = await _hubConnection.GetClipboardSendUrl();
            }
            else
            {
                _messenger.SendToast("Only text and images are supported", ToastType.Warning);
                return;
            }
        }
    }
}
