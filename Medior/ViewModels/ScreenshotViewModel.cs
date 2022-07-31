using Medior.Extensions;
using Medior.Models.Messages;
using Medior.Reactive;
using Medior.Services;
using Medior.Services.ScreenCapture;
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
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Medior.ViewModels
{
    public interface IScreenshotViewModel
    {
        ICommand CaptureCommand { get; }
        ImageSource? CurrentImage { get; }
        ICommand ShareCommand { get; }
    }
    public class ScreenshotViewModel : ObservableObjectEx, IScreenshotViewModel
    {
        private readonly RecyclableMemoryStreamManager _streamManager = new();
        private readonly IDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly ICapturePicker _picker;
        private readonly IWindowService _windowService;
        private readonly IApiService _apiService;
        private readonly ILogger<ScreenshotViewModel> _logger;

        public ScreenshotViewModel(
            ICapturePicker picker, 
            IDialogService dialogService, 
            IMessenger messenger,
            IApiService apiService,
            IWindowService windowService,
            ILogger<ScreenshotViewModel> logger)
        {
            _picker = picker;
            _dialogService = dialogService;
            _messenger = messenger;
            _windowService = windowService;
            _apiService = apiService;
            _logger = logger;
            CaptureCommand = new AsyncRelayCommand(Capture);
            ShareCommand = new AsyncRelayCommand(Share);

            _messenger.Register<PrintScreenInvokedMessage>(this, HandlePrintScreenInvoked);
        }

        public ICommand CaptureCommand { get; }

        public ImageSource? CurrentImage
        {
            get => Get<ImageSource>();
            set => Set(value);
        }

        private Bitmap? _currentBitmap;

        public ICommand ShareCommand { get; }

        public async Task Capture()
        {
            _currentBitmap?.Dispose();

            var result = _picker.GetScreenCapture();

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
        }

        public async Task Share()
        {
            try
            {
                if (_currentBitmap is null)
                {
                    await _dialogService.ShowError("Unexpected state.  Underlying bitmap is null.");
                    return;
                }

                using var ms = _streamManager.GetStream();
                _currentBitmap.Save(ms, ImageFormat.Jpeg);
                var result = await _apiService.UploadFile(ms.ToArray());

                if (!result.IsSuccess)
                {
                    await _dialogService.ShowError(result.Error!);
                    return;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sharing file.");
            }
        }

        private async void HandlePrintScreenInvoked(object recipient, PrintScreenInvokedMessage message)
        {
            await Capture();
        }
    }
}
