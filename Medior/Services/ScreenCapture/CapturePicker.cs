using Medior.Controls;
using Medior.Utilities;
using ScreenR.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Medior.Services.ScreenCapture
{
    public interface ICapturePicker
    {
        Result<Rectangle> GetCaptureArea();
        Result<Bitmap?> GetScreenCapture();
    }

    public class CapturePicker : ICapturePicker
    {
        private readonly IScreenGrabber _grabber;
        private readonly IWindowService _windowService;

        public CapturePicker(IScreenGrabber grabber, IWindowService windowService)
        {
            _grabber = grabber;
            _windowService = windowService;
        }

        public Result<Rectangle> GetCaptureArea()
        {
            using var _ = _windowService.HideMainWindow();

            var result = _grabber.GetScreenGrab();

            if (!result.IsSuccess)
            {
                return Result.Fail<Rectangle>(result.Exception!);
            }

            using var screenshot = result.Value!;
            var selectedArea = _windowService.ShowCapturePicker(screenshot);

            return Result.Ok(selectedArea);
        }

        public Result<Bitmap?> GetScreenCapture()
        {
            using var _ = _windowService.HideMainWindow();

            var result = _grabber.GetScreenGrab();

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
    }
}
