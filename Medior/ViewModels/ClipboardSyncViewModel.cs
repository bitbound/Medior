using CommunityToolkit.Mvvm.ComponentModel;
using Medior.Shared;
using Medior.Shared.Dtos;
using Medior.Shared.Dtos.Enums;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
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
        private readonly IClipboardApi _clipboardApi;
        private readonly IDesktopHubConnection _hubConnection;
        private readonly ILogger<ClipboardSyncViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly IQrCodeGenerator _qrCodeGenerator;

        [ObservableProperty]
        private ImageSource? _qrCodeImage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopySyncUrlCommand))]
        private string _syncUrl = string.Empty;

        public ClipboardSyncViewModel(
            IMessenger messenger,
            IDesktopHubConnection hubConnection,
            IQrCodeGenerator qrCodeGenerator,
            IClipboardApi clipApi,
            ILogger<ClipboardSyncViewModel> logger)
        {
            _messenger = messenger;
            _hubConnection = hubConnection;
            _qrCodeGenerator = qrCodeGenerator;
            _clipboardApi = clipApi;
            _logger = logger;
        }

        private bool IsCopyEnabled => !string.IsNullOrWhiteSpace(SyncUrl);


        [RelayCommand(CanExecute = nameof(IsCopyEnabled))]
        private void CopySyncUrl()
        {
            Clipboard.SetText(_syncUrl);
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }


        private Result<ClipboardContentDto> GetClipboardContent()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                var binary = Encoding.UTF8.GetBytes(text);
                var content = new ClipboardContentDto()
                {
                    Content = binary,
                    Type = ClipboardContentType.Text
                };
                return Result.Ok(content);

            }
            else if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                using var bitmap = image.ToBitmap();
                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);
                
                var content = new ClipboardContentDto()
                {
                    Content = ms.ToArray(),
                    Type = ClipboardContentType.Text
                };

                return Result.Ok(content);
            }
            else
            {
                return Result.Fail<ClipboardContentDto>("Only text and images are supported");
            }
        }

        [RelayCommand]
        private async Task Receive()
        {
            var receiveResult = await _hubConnection.GetClipboardReceiveUrl();
            if (!receiveResult.IsSuccess)
            {
                return;
            }

            SyncUrl = receiveResult.Value!;

            var codeResult = _qrCodeGenerator.GenerateCode(SyncUrl);
            if (!codeResult.IsSuccess)
            {
                _messenger.SendToast("Failed to generate QR code", ToastType.Error);
                return;
            }

            QrCodeImage = codeResult.Value!.ToBitmapImage(ImageFormat.Png);
        }

        [RelayCommand]
        private async Task Send()
        {
            var result = GetClipboardContent();
            if (!result.IsSuccess)
            {
                _messenger.SendToast(result.Error!, ToastType.Warning);
                return;
            }

            var sendResult = await _clipboardApi.SetClipboardContent(result.Value!);
            if (!sendResult.IsSuccess)
            {
                return;
            }

            SyncUrl = sendResult.Value!;

            var codeResult = _qrCodeGenerator.GenerateCode(SyncUrl);
            if (!codeResult.IsSuccess)
            {
                _messenger.SendToast("Failed to generate QR code", ToastType.Error);
                return;
            }

            QrCodeImage = codeResult.Value!.ToBitmapImage(ImageFormat.Png);
        }
    }
}
