using Medior.Extensions;
using Medior.Models.Messages;
using Medior.Reactive;
using Medior.Services;
using Medior.Services.ScreenCapture;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace Medior.ViewModels
{
    internal interface IScreenshotViewModel
    {
        ICommand CaptureCommand { get; }
        ImageSource? CurrentImage { get; }
    }
    internal class ScreenshotViewModel : ObservableObjectEx, IScreenshotViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IMessenger _messenger;
        private readonly ICapturePicker _picker;
        private readonly IWindowService _windowService;

        public ScreenshotViewModel(
                    ICapturePicker picker, 
            IDialogService dialogService, 
            IMessenger messenger,
            IWindowService windowService)
        {
            _picker = picker;
            _dialogService = dialogService;
            _messenger = messenger;
            _windowService = windowService;
            CaptureCommand = new AsyncRelayCommand(Capture);

            _messenger.Register<PrintScreenInvokedMessage>(this, HandlePrintScreenInvoked);
        }

        public ICommand CaptureCommand { get; }

        public ImageSource? CurrentImage
        {
            get => Get<ImageSource>();
            set => Set(value);
        }

        public async Task Capture()
        {
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

            _windowService.ShowMainWindow();
        }

        private async void HandlePrintScreenInvoked(object recipient, PrintScreenInvokedMessage message)
        {
            await Capture();
        }
    }
}
