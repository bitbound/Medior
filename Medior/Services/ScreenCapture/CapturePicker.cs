using Medior.Controls;
using Medior.Shared;
using Microsoft.Toolkit.Mvvm.Messaging;
using ScreenR.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Medior.Services.ScreenCapture
{
    public interface ICapturePicker
    {
        Result<Rectangle> GetCaptureArea();
        Result<Rectangle> GetCaptureAreaNoBackground();

        Result<Bitmap?> GetScreenCapture(bool captureCursor);
        Task<Result<Uri?>> GetScreenRecording(CancellationToken cancellationToken);
    }

    public class CapturePicker : ICapturePicker
    {
        private readonly IScreenGrabber _grabber;
        private readonly IScreenRecorder _screenRecorder;
        private readonly IFileSystem _fileSystem;
        private readonly ISystemTime _systemTime;
        private readonly IWindowService _windowService;
        public CapturePicker(
            IScreenGrabber grabber, 
            IWindowService windowService,
            IScreenRecorder screenRecorder,
            IFileSystem fileSystem,
            ISystemTime systemTime)
        {
            _grabber = grabber;
            _windowService = windowService;
            _screenRecorder = screenRecorder;
            _fileSystem = fileSystem;
            _systemTime = systemTime;
        }

        public Result<Rectangle> GetCaptureArea()
        {
            using var _ = _windowService.HideMainWindow();

            var result = _grabber.GetScreenGrab(false);

            if (!result.IsSuccess)
            {
                return Result.Fail<Rectangle>(result.Exception!);
            }

            using var screenshot = result.Value!;
            var selectedArea = _windowService.ShowCapturePicker(screenshot);

            return Result.Ok(selectedArea);
        }

        public Result<Rectangle> GetCaptureAreaNoBackground()
        {
            using var _ = _windowService.HideMainWindow();

            var selectedArea = _windowService.ShowCapturePicker();

            return Result.Ok(selectedArea);
        }

        public Result<Bitmap?> GetScreenCapture(bool captureCursor)
        {
            using var _ = _windowService.HideMainWindow();

            var result = _grabber.GetScreenGrab(captureCursor);

            if (!result.IsSuccess)
            {
                return Result.Fail<Bitmap?>(result.Exception!);
            }

            using var screenshot = result.Value!;

            var selectedArea = _windowService.ShowCapturePicker(screenshot);

            if (selectedArea.IsEmpty)
            {
                return Result.Ok<Bitmap?>(null);
            }

            var cropped = screenshot.Clone(selectedArea, screenshot.PixelFormat);
            return Result.Ok<Bitmap?>(cropped);
        }

        public async Task<Result<Uri?>> GetScreenRecording(CancellationToken cancellationToken)
        {
            using var _ = _windowService.HideMainWindow();
            var selectedArea = _windowService.ShowCapturePicker();

            if (selectedArea.IsEmpty)
            {
                return Result.Ok<Uri?>(null);
            }

            using var _2 = _windowService.ShowRecordingFrame(selectedArea);

            _screenRecorder.CleanupRecordings();
            var fileName = $"Recording_{_systemTime.Now:yyyy-MM-dd hh.mm.ss.fff}.mp4";
            var filePath = Path.Combine(AppConstants.RecordingsDirectory, fileName);
            using var fs = _fileSystem.CreateFileStream(filePath, FileMode.Create);
            var result = await _screenRecorder.CaptureVideo(selectedArea, 10, fs, cancellationToken);
            if (!result.IsSuccess)
            {
                return Result.Fail<Uri?>(result.Exception!);
            }
            return Result.Ok<Uri?>(new Uri(filePath));
        }
    }
}
