using Medior.Shared;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Entities.Enums;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class ClipboardSyncViewModel
    {
        private readonly IClipboardApi _clipboardApi;
        private readonly IDesktopHubConnection _hubConnection;
        private readonly ILogger<ClipboardSyncViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly IProcessService _processes;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        private readonly IServerUriProvider _serverUri;
        private readonly ISettings _settings;
        private readonly ISystemTime _systemTime;
        private readonly IUiDispatcher _uiDispatcher;
        private readonly IWindowService _windowService;


        [ObservableProperty]
        private int _receiptExpirationSeconds;

        [ObservableProperty]
        private Timer? _receiptExpirationTimer;

        private DateTimeOffset _receiptStartTime;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CopySyncUrlCommand))]
        private string _syncUrl = string.Empty;

        public ClipboardSyncViewModel(
            IMessenger messenger,
            IDesktopHubConnection hubConnection,
            IQrCodeGenerator qrCodeGenerator,
            IClipboardApi clipApi,
            IServerUriProvider serverUri,
            ISystemTime systemTime,
            IUiDispatcher uiDispatcher,
            IWindowService windowService,
            IProcessService processService,
            ISettings settings,
            ILogger<ClipboardSyncViewModel> logger)
        {
            _messenger = messenger;
            _hubConnection = hubConnection;
            _qrCodeGenerator = qrCodeGenerator;
            _clipboardApi = clipApi;
            _serverUri = serverUri;
            _systemTime = systemTime;
            _uiDispatcher = uiDispatcher;
            _windowService = windowService;
            _processes = processService;
            _settings = settings;
            _logger = logger;

            _messenger.Register<ClipboardReadyDto>(this, HandleClipboardReady);

            RefreshClips();

            ClipboardSaves.CollectionChanged += ClipboardSaves_CollectionChanged;
        }

        public ObservableCollectionEx<ClipboardSaveDto> ClipboardSaves { get; } = new();

        private bool IsCopyEnabled => !string.IsNullOrWhiteSpace(SyncUrl);

        [RelayCommand]
        public void GetQrCode(ClipboardSaveDto clip)
        {
            var url = $"{_serverUri.ServerUri}/clipboard-sync/{clip.Id}?accessToken={clip.AccessTokenView}";
            _windowService.ShowQrCode(url, "Share Link");
        }

        public void RefreshClips()
        {
            foreach (var clip in _settings.ClipboardSaves)
            {
                if (!ClipboardSaves.Any(x => x.Id == clip.Id))
                {
                    ClipboardSaves.Add(clip);
                }
            }
        }

        public async Task UpdateClipboardSave(ClipboardSaveDto dto)
        {
            _settings.ClipboardSaves = ClipboardSaves.ToArray();
            var result = await _clipboardApi.UpdateClip(dto);
            if (!result.IsSuccess)
            {
                _messenger.SendToast("Failed to update clip", ToastType.Error);
                return;
            }
            _messenger.SendToast("Clip updated", ToastType.Success);
        }

        [RelayCommand]
        private void CancelReceive()
        {
            ReceiptExpirationTimer?.Dispose();
            ReceiptExpirationTimer = null;
        }

        private void ClipboardSaves_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _settings.ClipboardSaves = ClipboardSaves.ToArray();
        }

        [RelayCommand]
        private void CopyLink(ClipboardSaveDto clip)
        {
            var url = $"{_serverUri.ServerUri}/clipboard-sync/{clip.Id}?accessToken={clip.AccessTokenView}";
            Clipboard.SetText(url);
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }

        [RelayCommand(CanExecute = nameof(IsCopyEnabled))]
        private void CopySyncUrl()
        {
            Clipboard.SetText(_syncUrl);
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }

        [RelayCommand]
        private async Task DeleteFile(ClipboardSaveDto clip)
        {
            var result = await _clipboardApi.DeleteClip(clip);

            if (result.IsSuccess)
            {
                ClipboardSaves.Remove(clip);
                _messenger.SendToast("File deleted", ToastType.Success);
            }
            else
            {
                _messenger.SendToast("File delete failed", ToastType.Error);
            }
        }

        private Result<ClipboardContentDto> GetClipboardContent()
        {
            if (Clipboard.ContainsText())
            {
                var text = Clipboard.GetText();
                var content = new ClipboardContentDto(text);
                return Result.Ok(content);

            }
            else if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                using var bitmap = image.ToBitmap();
                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);

                var content = new ClipboardContentDto(ms.ToArray());

                return Result.Ok(content);
            }
            else
            {
                return Result.Fail<ClipboardContentDto>("Only text and images are supported");
            }
        }

        private void HandleClipboardReady(object recipient, ClipboardReadyDto message)
        {
            try
            {
                _messenger.Send(new LoaderUpdateMessage()
                {
                    IsShown = true,
                    Text = "Receiving clipboard"
                });

                ReceiptExpirationTimer?.Dispose();
                ReceiptExpirationTimer = null;

                ClipboardSaves.Add(message.ClipboardSave);

                _messenger.SendToast("Clipboard content received", ToastType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while receiving clipboard.");
                _messenger.SendToast("Failed to receive clipboard", ToastType.Error);
            }
            finally
            {
                _messenger.Send(LoaderUpdateMessage.Empty);
            }
        }
        [RelayCommand]
        private void OpenLink(ClipboardSaveDto clip)
        {
            var url = $"{_serverUri.ServerUri}/clipboard-sync/{clip.Id}?accessToken={clip.AccessTokenView}";
            var psi = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true
            };
            _processes.Start(psi);
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

            _windowService.ShowQrCode(SyncUrl, "Receive Clipboard Link");

            _receiptStartTime = _systemTime.Now;
            ReceiptExpirationSeconds = 600;
            ReceiptExpirationTimer?.Dispose();
            ReceiptExpirationTimer = new Timer(1_000);
            ReceiptExpirationTimer.Elapsed += (s, e) =>
            {

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

                var sendResult = await _clipboardApi.SaveClipboardContent(result.Value!);
                if (!sendResult.IsSuccess)
                {
                    return;
                }

                ClipboardSaves.Add(sendResult.Value!);
                _messenger.SendToast("Clipboard saved", ToastType.Success);
            }
            finally
            {
                _messenger.Send(LoaderUpdateMessage.Empty);
            }
        }
    }
}
