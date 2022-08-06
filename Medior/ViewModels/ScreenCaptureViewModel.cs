using Medior.Shared.Helpers;
using Medior.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Clipboard = System.Windows.Forms.Clipboard;

namespace Medior.ViewModels
{
    public partial class ScreenCaptureViewModel : ObservableObjectEx
    {
        private readonly IApiService _apiService;
        private readonly IDialogService _dialogService;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<ScreenCaptureViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly ICapturePicker _picker;
        private readonly IProcessService _processService;
        private readonly ISettings _settings;
        private readonly ISystemTime _systemTime;
        private readonly IWindowService _windowService;

        [ObservableProperty]
        private string? _captureViewUrl;

        private Bitmap? _currentBitmap;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHintTextVisible))]
        private ImageSource? _currentImage;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsHintTextVisible))]
        private Uri? _currentRecording;

        [ObservableProperty]
        private bool _isRecordingInProgress;

        private CancellationTokenSource? _recordingCts;

        public ScreenCaptureViewModel(
            ICapturePicker picker,
            IDialogService dialogService,
            IMessenger messenger,
            IApiService apiService,
            IWindowService windowService,
            ISettings settings,
            ISystemTime systemTime,
            IProcessService processService,
            IFileSystem fileSystem,
            ILogger<ScreenCaptureViewModel> logger)
        {
            _picker = picker;
            _dialogService = dialogService;
            _messenger = messenger;
            _windowService = windowService;
            _apiService = apiService;
            _logger = logger;
            _settings = settings;
            _processService = processService;
            _fileSystem = fileSystem;
            _systemTime = systemTime;

            _messenger.Register<GenericMessage<ScreenCaptureRequestKind>>(this, HandleScreenCaptureRequest);
            _messenger.Register<StopRecordingRequested>(this, HandleStopRecordingRequested);
        }
        public bool IsHintTextVisible =>
            CurrentImage is null &&
            CurrentRecording is null &&
            !IsRecordingInProgress;


        [RelayCommand]
        private async Task Capture()
        {
            await CaptureImpl(false);
        }

        private async Task CaptureImpl(bool captureCursor)
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
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }

        [RelayCommand]
        private async Task CopyCapture()
        {
            if (_currentBitmap is not null)
            {
                Clipboard.SetImage(_currentBitmap);
                _messenger.SendToast("Copied to clipboard", ToastType.Success);
            }
            else if (CurrentRecording is not null)
            {
                var collection = new StringCollection
                {
                    CurrentRecording.LocalPath
                };
                Clipboard.SetFileDropList(collection);
                _messenger.SendToast("Copied file to clipboard", ToastType.Success);
            }
            else
            {
                await _dialogService.ShowError("Unexpected state.  No image or video to copy.");
            }
        }

        [RelayCommand]
        private void CopyViewUrl()
        {
            Clipboard.SetText(CaptureViewUrl);
            _messenger.SendToast("Copied to clipboard", ToastType.Success);
        }

        [RelayCommand]
        private async Task EditCapture()
        {
            if (CurrentImage is not null)
            {
                await EditImage();
            }
            else if (CurrentRecording is not null)
            {
                await EditRecording();
            }
            else
            {
                await _dialogService.ShowError("Unexpected state.  Nothing to edit..");
                return;
            }
        }

        private async Task EditImage()
        {
            var status = await Launcher.QueryUriSupportAsync(new Uri("ms-screensketch:"), LaunchQuerySupportType.Uri);
            if (status != LaunchQuerySupportStatus.Available)
            {
                _messenger.SendToast("Snipping Tool app not found", ToastType.Warning);
                return;
            }

            if (_currentBitmap is null)
            {
                await _dialogService.ShowError("Unexpected state.  Bitmap is null.");
                return;
            }

            _fileSystem.CleanupTempFiles();

            var filePath = Path.Combine(AppConstants.ImagesDirectory, $"{Guid.NewGuid()}.png");
            _currentBitmap.Save(filePath, ImageFormat.Png);

            var storageFile = await _fileSystem.GetFileFromPathAsync(filePath);
            var token = _fileSystem.AddSharedStorageFile(storageFile);
            var launchResult = await _processService.LaunchUri(new Uri($"ms-screensketch:edit?sharedAccessToken={token}"));

            if (!launchResult ||
                !await WaitHelper.WaitForAsync(() => IsScreenSketchOpen(), TimeSpan.FromSeconds(10)))
            {
                _messenger.SendToast("Failed to launch Snipping Tool", ToastType.Warning);
                return;
            }

            Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged -= GetClipboardContent;
            Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged += GetClipboardContent;
        }

        private async Task EditRecording()
        {
            var status = await _processService.QueryUriSupportAsync(new Uri("ms-photos:"), LaunchQuerySupportType.Uri);
            if (status != LaunchQuerySupportStatus.Available)
            {
                _messenger.SendToast("Photos app not found", ToastType.Warning);
                return;
            }

            if (CurrentRecording is null)
            {
                await _dialogService.ShowError("Unexpected state.  Recording is null.");
                return;
            }

            var fileExt = Path.GetExtension(CurrentRecording.LocalPath);
            var tempFile = $"{Path.GetFileNameWithoutExtension(CurrentRecording.LocalPath)}-Temp{fileExt}";
            var tempPath = Path.Combine(AppConstants.RecordingsDirectory, tempFile);
            _fileSystem.CopyFile(CurrentRecording.LocalPath, tempPath, true);
            var storageFile = await _fileSystem.GetFileFromPathAsync(tempPath);
            var token = _fileSystem.AddSharedStorageFile(storageFile);
            var launchResult = await _processService.LaunchUri(new Uri($"ms-photos:videoedit?InputToken={token}"));

            if (!launchResult)
            {
                _messenger.SendToast("Failed to launch Photos", ToastType.Warning);
            }
        }
        [RelayCommand]
        private void GenerateQrCode()
        {
            if (string.IsNullOrWhiteSpace(CaptureViewUrl))
            {
                _logger.LogWarning("{captureViewUrl} shouldn't be empty here.", CaptureViewUrl);
                return;
            }

            if (CurrentImage is not null)
            {
                _windowService.ShowQrCode(CaptureViewUrl, "View Screenshot");
            }
            else if (CurrentRecording is not null)
            {
                _windowService.ShowQrCode(CaptureViewUrl, "View Recording");
            }
            else
            {
                _logger.LogWarning("Expected a screenshot or recording.");
                _messenger.SendToast("Unexpected state", ToastType.Warning);
            }
        }

        private async void GetClipboardContent(object? sender, object e)
        {
            try
            {
                if (!IsScreenSketchOpen())
                {
                    Windows.ApplicationModel.DataTransfer.Clipboard.ContentChanged -= GetClipboardContent;
                    return;
                }

                var content = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();

                if (!content.AvailableFormats.Contains("Bitmap"))
                {
                    return;
                }

                var bitmapStreamRef = await content.GetBitmapAsync();
                using var rtStream = await bitmapStreamRef.OpenReadAsync();
                using var stream = rtStream.AsStream();
                using var rgb32Bpp = new Bitmap(stream);

                _currentBitmap?.Dispose();
                _currentBitmap = new Bitmap(rgb32Bpp.Width, rgb32Bpp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using var graphics = Graphics.FromImage(_currentBitmap);
                graphics.DrawImage(rgb32Bpp, Point.Empty);

                CurrentImage = _currentBitmap.ToBitmapImage(ImageFormat.Png);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting clipboard content.");
            }
        }

        private async void HandleScreenCaptureRequest(object recipient, GenericMessage<ScreenCaptureRequestKind> message)
        {
            switch (message.Value)
            {
                case ScreenCaptureRequestKind.Snip:
                    await CaptureImpl(true);
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

        private bool IsScreenSketchOpen()
        {
            return _processService
                .GetProcessesByName("ScreenSketch")
                .Any();
        }
        [RelayCommand]
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

        [RelayCommand]
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
                    _messenger.SendToast("Image saved", ToastType.Success);
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
                    _messenger.SendToast("Video saved", ToastType.Success);
                    
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while saving capture.");
                await _dialogService.ShowError(ex);
            }
        }

        [RelayCommand]
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

        [RelayCommand]
        private void StopVideoCapture()
        {
            _recordingCts?.Cancel();
            _recordingCts?.Dispose();
            IsRecordingInProgress = false;
        }
    }
}
