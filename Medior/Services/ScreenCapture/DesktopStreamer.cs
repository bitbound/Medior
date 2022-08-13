using Medior.Dtos;
using Medior.Models;
using Medior.Shared.Services;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Medior.Services.ScreenCapture
{
    public interface IDesktopStreamer
    {
        event EventHandler? LoopIncremented;
        event EventHandler<SentFrame>? FrameSent;

        IAsyncEnumerable<DesktopFrameChunk> GetDesktopStream(CancellationToken cancellationToken);
        IEnumerable<DisplayInfo> GetDisplays();

        void SetActiveDisplay(string deviceName);
        void FrameReceived();
    }
    public class DesktopStreamer : IDesktopStreamer
    {
        private readonly RecyclableMemoryStreamManager _streamManager = new();
        private readonly IScreenGrabber _screenGrabber;
        private readonly ILogger<DesktopStreamer> _logger;
        private readonly ISystemTime _time;
        private readonly IBitmapUtility _bitmapUtility;
        private readonly SemaphoreSlim _sentFramesSignal = new(10, 10);
        private string _activeDisplay;
        private bool _forceFullscreen = true;

        public event EventHandler? LoopIncremented;
        public event EventHandler<SentFrame>? FrameSent;

        public DesktopStreamer(
            IScreenGrabber screenGrabber,
            IBitmapUtility bitmapUtility,
            ISystemTime time,
            ILogger<DesktopStreamer> logger)
        {
            _screenGrabber = screenGrabber;
            _logger = logger;
            _time = time;
            _bitmapUtility = bitmapUtility;

            _activeDisplay = _screenGrabber
                .GetDisplays()
                .FirstOrDefault(x => x.IsPrimary)
                ?.DeviceName ?? "";
        }

        public async IAsyncEnumerable<DesktopFrameChunk> GetDesktopStream(
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            var buffer = new byte[50_000];
            Bitmap currentFrame = new(0,0);
            Bitmap previousFrame = new(0,0);

            while (!cancellationToken.IsCancellationRequested)
            {
                using var encodeStream = _streamManager.GetStream();
                Rectangle diffArea;
                
                try
                {

                    LoopIncremented?.Invoke(this, EventArgs.Empty);

                    Array.Clear(buffer);

                    previousFrame?.Dispose();
                    previousFrame = currentFrame;

                    var result = _screenGrabber.GetScreenGrab(_activeDisplay, false);
                    if (result.IsSuccess && result.Value is not null)
                    {
                        currentFrame = result.Value;
                    }
                    else
                    {
                        var err = result.Error ?? "Null frame returned.";
                        _logger.LogError("Screen grab failed.  Error: {msg}", err);
                    }

                    var diffResult = _bitmapUtility.GetDiffArea(currentFrame, previousFrame, _forceFullscreen);

                    if (!diffResult.IsSuccess)
                    {
                        _logger.LogError("Get image diff failed.  Error: {msg}", diffResult.Error);
                        continue;
                    }

                    diffArea = diffResult.Value;
                    _forceFullscreen = false;

                    if (diffArea.IsEmpty)
                    {
                        continue;
                    }


                    using var cropped = _bitmapUtility.CropBitmap(currentFrame, diffArea);
                    cropped.Save(encodeStream, ImageFormat.Jpeg);
                    encodeStream.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    Array.Clear(buffer);
                    _logger.LogError(ex, "Error while streaming desktop.");
                    continue;
                }

                await _sentFramesSignal.WaitAsync(cancellationToken);

                var chunks = encodeStream.ToArray().Chunk(50_000).ToArray();
                var lastChunk = chunks.Length - 1;

                for (var i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];

                    var desktopChunk = new DesktopFrameChunk()
                    {
                        Area = diffArea,
                        ImageBytes = chunk,
                        EndOfFrame = i == lastChunk
                    };

                    yield return desktopChunk;
                }

                FrameSent?.Invoke(this, new(encodeStream.Length, _time.Now));
            }
            _logger.LogInformation("Streaming ended.");
        }

        public IEnumerable<DisplayInfo> GetDisplays()
        {
            return _screenGrabber.GetDisplays();
        }

        public void SetActiveDisplay(string deviceName)
        {
            _activeDisplay = deviceName;
        }

        public void FrameReceived()
        {
            _sentFramesSignal.Release();
        }
    }
}
