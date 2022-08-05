using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Medior.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;

namespace Medior.ViewModels
{
    public interface IScreenCaptureViewModel
    {
        ICommand CaptureCommand { get; }
        string? CaptureViewUrl { get; set; }
        ICommand CopyCaptureCommand { get; }
        ICommand CopyViewUrlCommand { get; }
        ImageSource? CurrentImage { get; }
        Uri? CurrentRecording { get; }
        bool IsHintTextVisible { get; }
        bool IsRecordingInProgress { get; }
        ICommand RecordCommand { get; }
        ICommand SaveCommand { get; }
        ICommand ShareCommand { get; }
        ICommand StopVideoCaptureCommand { get; }
    }
    public class ScreenCaptureViewModel : ObservableObjectEx, IScreenCaptureViewModel
    {
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ScreenCaptureViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly ICapturePicker _picker;
        private readonly ISettings _settings;
        private readonly ISystemTime _systemTime;
        private readonly IWindowService _windowService;
        private Bitmap? _currentBitmap;
        private CancellationTokenSource? _recordingCts;

        public ScreenCaptureViewModel(
            ICapturePicker picker,
            IDialogService dialogService,
            IMessenger messenger,
            IApiService apiService,
            IWindowService windowService,
            ISettings settings,
            ISystemTime systemTime,
            ILogger<ScreenCaptureViewModel> logger)
        {
            _picker = picker;
            _dialogService = dialogService;
            _messenger = messenger;
            _windowService = windowService;
            _apiService = apiService;
            _logger = logger;
            _settings = settings;
            _systemTime = systemTime;

            CaptureCommand = new AsyncRelayCommand(() => Capture(false));
            RecordCommand = new AsyncRelayCommand(Record);
            ShareCommand = new AsyncRelayCommand(Share);
            CopyViewUrlCommand = new RelayCommand(CopyUrl);
            CopyCaptureCommand = new AsyncRelayCommand(CopyCapture);
            StopVideoCaptureCommand = new RelayCommand(StopVideoCapture);
            SaveCommand = new AsyncRelayCommand(Save);

            _messenger.Register<GenericMessage<ScreenCaptureRequestKind>>(this, HandleScreenCaptureRequest);
            _messenger.Register<StopRecordingRequested>(this, HandleStopRecordingRequested);
        }


        public ICommand CaptureCommand { get; }

        public string? CaptureViewUrl
        {
            get => Get<string>();
            set => Set(value);
        }

        public ICommand CopyCaptureCommand { get; }

        public ICommand CopyViewUrlCommand { get; }

        public ImageSource? CurrentImage
        {
            get => Get<ImageSource>();
            set
            {
                Set(value);
                OnPropertyChanged(nameof(IsHintTextVisible));
            }
        }

        public Uri? CurrentRecording
        {
            get => Get<Uri?>();
            set
            {
                Set(value);
                OnPropertyChanged(nameof(IsHintTextVisible));
            }
        }

        public bool IsHintTextVisible =>
            CurrentImage is null &&
            CurrentRecording is null &&
            !IsRecordingInProgress;

        public bool IsRecordingInProgress
        {
            get => Get<bool>();
            set => Set(value);
        }

        public ICommand RecordCommand { get; }

        public ICommand SaveCommand { get; }
        public ICommand ShareCommand { get; }

        public ICommand StopVideoCaptureCommand { get; }
        private async Task Capture(bool captureCursor)
        {
            ResetCaptureState();

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

            Clipboard.SetImage(_currentBitmap);
            _messenger.Send(new ToastMessage("Copied to clipboard", ToastType.Success));
        }

        private async Task CopyCapture()
        {
            if (_currentBitmap is not null)
            {
                Clipboard.SetImage(_currentBitmap);
                _messenger.Send(new ToastMessage("Copied to clipboard", ToastType.Success));
            }
            else if (CurrentRecording is not null)
            {
                var collection = new StringCollection
                {
                    CurrentRecording.LocalPath
                };
                Clipboard.SetFileDropList(collection);
                _messenger.Send(new ToastMessage("Copied file to clipboard", ToastType.Success));
            }
            else
            {
                await _dialogService.ShowError("Unexpected state.  No image or video to copy.");
            }
        }

        private void CopyUrl()
        {
            Clipboard.SetText(CaptureViewUrl);
            _messenger.Send(new ToastMessage("Copied to clipboard", ToastType.Success));
        }

        private async void HandleScreenCaptureRequest(object recipient, GenericMessage<ScreenCaptureRequestKind> message)
        {
            switch (message.Value)
            {
                case ScreenCaptureRequestKind.Snip:
                    await Capture(true);
                    break;
                case ScreenCaptureRequestKind.Record:
                    await Record();
                    break;
                default:
                    break;
            }
            _windowService.ShowMainWindow();
        }


        private void HandleStopRecordingRequested(object recipient, StopRecordingRequested message)
        {
            _recordingCts?.Cancel();
        }

        private async Task Record()
        {
            try
            {
                IsRecordingInProgress = true;

                ResetCaptureState();

                _recordingCts = new CancellationTokenSource();

                var result = await _picker.GetScreenRecording(_recordingCts.Token);
                if (!result.IsSuccess)
                {
                    await _dialogService.ShowError(result.Exception!);
                    return;
                }

                CurrentRecording = result.Value;
                _windowService.ShowMainWindow();
            }
            finally
            {
                IsRecordingInProgress = false;
            }
        }

        private void ResetCaptureState()
        {
            CaptureViewUrl = null;
            _currentBitmap?.Dispose();
            _currentBitmap = null;
            CurrentImage = null;
            CurrentRecording = null;
        }

        private async Task Save()
        {
            try
            {
                if (_currentBitmap is null && CurrentRecording is null)
                {
                    await _dialogService.ShowError("Unexpected state.  No image or video to save.");
                    return;
                }

                if (_currentBitmap is not null)
                {
                    var sfd = new SaveFileDialog()
                    {
                        Filter = "Image Files (*.jpg)|*.jpg",
                        AddExtension = true,
                        DefaultExt = ".jpg",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                    };

                    _ = sfd.ShowDialog();
                    if (string.IsNullOrWhiteSpace(sfd.FileName))
                    {
                        return;
                    }
                    using var fs = new FileStream(sfd.FileName, FileMode.Create);
                    _currentBitmap.Save(fs, ImageFormat.Jpeg);
                    _messenger.Send(new ToastMessage("Image saved", ToastType.Success));
                }
                else if (CurrentRecording is not null)
                {
                    var sfd = new SaveFileDialog()
                    {
                        Filter = "Video Files (*.mp4)|*.mp4",
                        AddExtension = true,
                        DefaultExt = ".mp4",
                        InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos)
                    };

                    _ = sfd.ShowDialog();
                    if (string.IsNullOrWhiteSpace(sfd.FileName))
                    {
                        return;
                    }

                    File.Copy(CurrentRecording.LocalPath, sfd.FileName, true);
                    _messenger.Send(new ToastMessage("Video saved", ToastType.Success));
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving capture.");
                await _dialogService.ShowError(ex);
            }
        }

        private async Task Share()
        {
            if (_currentBitmap is not null)
            {
                await ShareScreenshot();
            }
            else if (CurrentRecording is not null)
            {
                await ShareVideo();
            }
            else
            {
                await _dialogService.ShowError("Unexpected state.  No image or video to share.");
            }
        }

        private async Task ShareScreenshot()
        {
            try
            {
                if (_currentBitmap is null)
                {
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
                fileStream.Seek(0, SeekOrigin.Begin);

                var totalSize = fileStream.Length;

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

                var fileName = $"Medior_Screenshot_{_systemTime.Now:yyyy-MM-dd hh.mm.ss.fff}.jpg";
                var result = await _apiService.UploadFile(fileStream, fileName);

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
                await _dialogService.ShowError(ex);
            }
            finally
            {
                _messenger.Send(new LoaderUpdate());
            }
        }

        private async Task ShareVideo()
        {
            try
            {
                if (CurrentRecording is null)
                {
                    return;
                }

                _messenger.Send(new LoaderUpdate()
                {
                    IsShown = true,
                    Text = "Uploading video",
                    Type = LoaderType.Progress
                });

                using var reactiveStream = new ReactiveStream();
                using var fileStream = new FileStream(CurrentRecording.LocalPath, FileMode.Open, FileAccess.Read);
                await fileStream.CopyToAsync(reactiveStream);
                reactiveStream.Seek(0, SeekOrigin.Begin);

                var totalSize = fileStream.Length;

                reactiveStream.TotalBytesReadChanged += (sender, read) =>
                {
                    _messenger.Send(new LoaderUpdate()
                    {
                        IsShown = true,
                        Text = "Uploading video",
                        Type = LoaderType.Progress,
                        LoaderProgress = (double)read / totalSize
                    });
                };

                var fileName = $"Medior_Recording_{_systemTime.Now:yyyy-MM-dd hh.mm.ss.fff}.mp4";
                var result = await _apiService.UploadFile(reactiveStream, fileName);

                if (!result.IsSuccess)
                {
                    await _dialogService.ShowError(result.Error!);
                    return;
                }

                CaptureViewUrl = $"{_settings.ServerUri}/media-viewer/{result.Value!.Id}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sharing video.");
                await _dialogService.ShowError(ex);
            }
            finally
            {
                _messenger.Send(new LoaderUpdate());
            }
        }
        private void StopVideoCapture()
        {
            _recordingCts?.Cancel();
            _recordingCts?.Dispose();
            IsRecordingInProgress = false;
        }
    }
}
