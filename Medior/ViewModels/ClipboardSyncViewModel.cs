using CommunityToolkit.Mvvm.ComponentModel;
using Medior.Shared;
using Medior.Shared.Dtos;
using Medior.Shared.Dtos.Enums;
using Medior.Shared.Interfaces;
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
using System.Timers;
using System.Windows;
using System.Windows.Media;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class ClipboardSyncViewModel
    {
        private readonly IClipboardApi _clipboardApi;
        private readonly IServerUriProvider _serverUri;
        private readonly ISystemTime _systemTime;
        private readonly IUiDispatcher _uiDispatcher;
        private readonly IDesktopHubConnection _hubConnection;
        private readonly ILogger<ClipboardSyncViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly IQrCodeGenerator _qrCodeGenerator;

        [ObservableProperty]
        private ImageSource? _qrCodeImage;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopySyncUrlCommand))]
        private string _syncUrl = string.Empty;

        [ObservableProperty]
        private Timer? _receiptExpirationTimer;

        [ObservableProperty]
        private int _receiptExpirationSeconds;
        private DateTimeOffset _receiptStartTime;

        public ClipboardSyncViewModel(
            IMessenger messenger,
            IDesktopHubConnection hubConnection,
            IQrCodeGenerator qrCodeGenerator,
            IClipboardApi clipApi,
            IServerUriProvider serverUri,
            ISystemTime systemTime,
            IUiDispatcher uiDispatcher,
            ILogger<ClipboardSyncViewModel> logger)
        {
            _messenger = messenger;
            _hubConnection = hubConnection;
            _qrCodeGenerator = qrCodeGenerator;
            _clipboardApi = clipApi;
            _serverUri = serverUri;
            _systemTime = systemTime;
            _uiDispatcher = uiDispatcher;
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
                    Type = ClipboardContentType.Bitmap
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
            var receiptResult = await _hubConnection.GetClipboardReceiptToken();
            if (!receiptResult.IsSuccess)
            {
                return;
            }
           
            SyncUrl = $"{_serverUri.ServerUri}/clipboard-sync?receiptToken={receiptResult.Value!}";

            var codeResult = _qrCodeGenerator.GenerateCode(SyncUrl);
            if (!codeResult.IsSuccess)
            {
                _messenger.SendToast("Failed to generate QR code", ToastType.Error);
                return;
            }

            QrCodeImage = codeResult.Value!.ToBitmapImage(ImageFormat.Png);

            _receiptStartTime = _systemTime.Now;
            ReceiptExpirationSeconds = 600;
            ReceiptExpirationTimer?.Dispose();
            ReceiptExpirationTimer = new Timer(1_000);
            ReceiptExpirationTimer.Elapsed += (s, e) => {

                _uiDispatcher.Invoke(() =>
                {
                    var elapsed = _systemTime.Now - _receiptStartTime;
                    if (elapsed.TotalMinutes >= 10)
                    {
                        ReceiptExpirationTimer?.Dispose();
                        ReceiptExpirationTimer = null;
                        _messenger.SendToast("Receive link has expired", ToastType.Warning);
                        return;
                    }

                    ReceiptExpirationSeconds = (int)(600 - elapsed.TotalSeconds);

                });
            };
            ReceiptExpirationTimer.Start();
        }

        [RelayCommand]
        private async Task Send()
        {
            try
            {
                _messenger.Send(new LoaderUpdateMessage()
                {
                    IsShown = true,
                    Text = "Sending clipboard"
                });


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
            finally
            {
                _messenger.Send(LoaderUpdateMessage.Empty);
            }
        }
    }
}
