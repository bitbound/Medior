using Medior.Extensions;
using Medior.Models.Messages;
using Medior.Reactive;
using Medior.Services.ScreenCapture;
using Medior.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Medior.ViewModels
{
    public interface IScreenCaptureViewModel
    {
        ICommand CaptureCommand { get; }
        ICommand RecordCommand { get; }
        ICommand CopyImageCommand { get; }
        ICommand CopyViewUrlCommand { get; }
        ImageSource? CurrentImage { get; }
        string? CaptureViewUrl { get; set; }
        ICommand ShareCommand { get; }
    }
    public class ScreenCaptureViewModel : ObservableObjectEx, IScreenCaptureViewModel
    {
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ScreenCaptureViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly ICapturePicker _picker;
        private readonly ISettings _settings;
        private readonly IWindowService _windowService;
        private Bitmap? _currentBitmap;

        public ScreenCaptureViewModel(
                    ICapturePicker picker, 
            IDialogService dialogService, 
            IMessenger messenger,
            IApiService apiService,
            IWindowService windowService,
            ISettings settings,
            ILogger<ScreenCaptureViewModel> logger)
        {
            _picker = picker;
            _dialogService = dialogService;
            _messenger = messenger;
            _windowService = windowService;
            _apiService = apiService;
            _logger = logger;
            _settings = settings;
            CaptureCommand = new AsyncRelayCommand(() => Capture(false));
            RecordCommand = new AsyncRelayCommand(Record);
            ShareCommand = new AsyncRelayCommand(Share);
            CopyViewUrlCommand = new RelayCommand(CopyUrl);
            CopyImageCommand = new RelayCommand(CopyImage);

            _messenger.Register<PrintScreenInvokedMessage>(this, HandlePrintScreenInvoked);
        }

        public ICommand CaptureCommand { get; }

        public ICommand RecordCommand { get; }

        public ICommand CopyImageCommand { get; }

        public ICommand CopyViewUrlCommand { get; }

        public ImageSource? CurrentImage
        {
            get => Get<ImageSource>();
            set => Set(value);
        }
        public string? CaptureViewUrl
        {
            get => Get<string>();
            set => Set(value);
        }

        public ICommand ShareCommand { get; }

        private async Task Record()
        {
            CaptureViewUrl = null;
            _currentBitmap?.Dispose();
            _currentBitmap = null;

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var result = await _picker.GetScreenRecording(cts.Token);
        }

        private async Task Capture(bool captureCursor)
        {
            CaptureViewUrl = null;
            _currentBitmap?.Dispose();

            var result = _picker.GetScreenCapture(captureCursor);

            if (!result.IsSuccess)
            {
                await _dialogService.ShowError(result.Exception!);
                return;
            }

            if (result.Value is null)
            {
                return;
            }

            CurrentImage = result.Value?.ToBitmapImage(ImageFormat.Png);
            _currentBitmap = result.Value;

            _windowService.ShowMainWindow();

            System.Windows.Forms.Clipboard.SetImage(_currentBitmap);
            _messenger.Send(new ToastMessage("Copied to clipboard", ToastType.Success));
        }

        private void CopyImage()
        {
            System.Windows.Forms.Clipboard.SetImage(_currentBitmap);
            _messenger.Send(new ToastMessage("Copied to clipboard", ToastType.Success));
        }

        private void CopyUrl()
        {
            System.Windows.Forms.Clipboard.SetText(CaptureViewUrl);
            _messenger.Send(new ToastMessage("Copied to clipboard", ToastType.Success));
        }

        private async void HandlePrintScreenInvoked(object recipient, PrintScreenInvokedMessage message)
        {
            await Capture(true);
        }

        private async Task Share()
        {
            try
            {
                if (_currentBitmap is null)
                {
                    await _dialogService.ShowError("Unexpected state.  Underlying bitmap is null.");
                    return;
                }

                _messenger.Send(new LoaderUpdate()
                {
                    IsShown = true,
                    Text = "Uploading image",
                    Type = LoaderType.Progress
                });

                using var fileStream = new ReactiveStream();
                _currentBitmap.Save(fileStream, ImageFormat.Jpeg);

                var totalSize = fileStream.Length;
                fileStream.Seek(0, System.IO.SeekOrigin.Begin);

                fileStream.TotalBytesReadChanged += (sender, read) =>
                {
                    _messenger.Send(new LoaderUpdate()
                    {
                        IsShown = true,
                        Text = "Uploading image",
                        Type = LoaderType.Progress,
                        LoaderProgress = (double)read / totalSize
                    });
                };
                
                var result = await _apiService.UploadFile(fileStream, "Medior_Screenshot.jpg");

                if (!result.IsSuccess)
                {
                    await _dialogService.ShowError(result.Error!);
                    return;
                }

                CaptureViewUrl = $"{_settings.ServerUri}/media-viewer/{result.Value!.Id}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sharing file.");
            }
            finally
            {
                _messenger.Send(new LoaderUpdate());
            }
        }
    }
}
