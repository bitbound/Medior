using Medior.Controls;
using Medior.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services.ScreenCapture
{
    internal interface ICapturePicker
    {
        Result<Rectangle> GetCaptureArea();
        Result<Bitmap?> GetScreenCapture();
    }

    internal class CapturePicker : ICapturePicker
    {
        private readonly IScreenGrabber _grabber;

        public CapturePicker(IScreenGrabber grabber)
        {
            _grabber = grabber;
        }

        public Result<Rectangle> GetCaptureArea()
        {
            var result = _grabber.GetScreenGrab();

            if (!result.IsSuccess)
            {
                return Result.Fail<Rectangle>(result.Exception!);
            }

            var window = new CapturePickerWindow(result.Value!);
            window.ShowDialog();
            return Result.Ok(window.SelectedArea);
        }

        public Result<Bitmap?> GetScreenCapture()
        {
            var result = _grabber.GetScreenGrab();

            if (!result.IsSuccess)
            {
                return Result.Fail<Bitmap?>(result.Exception!);
            }

            var screenshot = result.Value!;

            var window = new CapturePickerWindow(screenshot);
            window.ShowDialog();

            if (window.SelectedArea.IsEmpty)
            {
                return Result.Ok<Bitmap?>(null);
            }
            var cropped = screenshot.Clone(window.SelectedArea, screenshot.PixelFormat);
            return Result.Ok<Bitmap?>(cropped);
        }
    }
}
