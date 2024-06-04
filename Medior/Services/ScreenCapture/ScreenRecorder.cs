using Medior.Models;
using Medior.Shared;
using Medior.Shared.IO;
using Medior.Shared.Models;
using Medior.Shared.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;

namespace Medior.Services.ScreenCapture;

public interface IScreenRecorder
{
    Task<Result> CaptureVideo(
        DisplayInfo display,
        int frameRate,
        Stream destinationStream,
        CancellationToken cancellationToken);

    Task<Result> CaptureVideo(
         Rectangle captureArea,
         int frameRate,
         Stream destinationStream,
         CancellationToken cancellationToken);
    IAsyncEnumerable<VideoChunk> StreamVideo(Rectangle selectedArea, int frameRate, CancellationToken cancellationToken);
}

public class ScreenRecorder : IScreenRecorder
{
    //private static readonly Guid MFTranscodeContainerType_MPEG4 = new("DC6CD05D-B9D0-40ef-BD35-FA622C1AB28A");
    private static readonly Guid MFTranscodeContainerType_FMPEG4 = new("9ba876f1-419f-4b77-a1e0-35959d9d4004");


    private readonly IScreenGrabber _grabber;
    private readonly ISystemTime _systemTime;
    private readonly ILogger<ScreenRecorder> _logger;

    public ScreenRecorder(
        IScreenGrabber screenGrabber,
        ISystemTime systemTime,
        ILogger<ScreenRecorder> logger)
    {
        _grabber = screenGrabber;
        _systemTime = systemTime;
        _logger = logger;
    }

    public async Task<Result> CaptureVideo(
        Rectangle captureArea, 
        int frameRate, 
        Stream destinationStream, 
        CancellationToken cancellationToken)
    {
        return await CaptureVideoImpl(captureArea, frameRate, destinationStream, false, cancellationToken);
    }

    public async Task<Result> CaptureVideo(
        DisplayInfo display,
        int frameRate,
        Stream destinationStream,
        CancellationToken cancellationToken)
    {

        var captureArea = new Rectangle(Point.Empty, display.MonitorArea.Size);
        return await CaptureVideoImpl(captureArea, frameRate, destinationStream, false, cancellationToken);
    }

    public IAsyncEnumerable<VideoChunk> StreamVideo(
        Rectangle captureArea, 
        int frameRate,
        CancellationToken cancellationToken)
    {
        var emitStream = new EmittableVideoStream();

        _ = Task.Run(() => CaptureVideoImpl(captureArea, frameRate, emitStream, true, cancellationToken), cancellationToken);

        return emitStream.GetRedirectedStream(cancellationToken);
    }

    private async Task<Result> CaptureVideoImpl(
        Rectangle captureArea,
        int frameRate,
        Stream destinationStream,
        bool makeStreamable,
        CancellationToken cancellationToken)
    {
        try
        {
            captureArea.Width = captureArea.Width % 2 == 0 ? captureArea.Width : captureArea.Width + 1;
            captureArea.Height = captureArea.Height % 2 == 0 ? captureArea.Height : captureArea.Height + 1;

            var size = captureArea.Width * 4 * captureArea.Height;

            var tempBuffer = new byte[size];

            var sourceVideoProperties = VideoEncodingProperties.CreateUncompressed(
                MediaEncodingSubtypes.Argb32,
                (uint)captureArea.Width,
                (uint)captureArea.Height);

            var videoDescriptor = new VideoStreamDescriptor(sourceVideoProperties);

            var mediaStreamSource = new MediaStreamSource(videoDescriptor)
            {
                BufferTime = TimeSpan.Zero
            };

            var stopwatch = Stopwatch.StartNew();

            mediaStreamSource.Starting += (sender, args) =>
            {
                args.Request.SetActualStartPosition(stopwatch.Elapsed);
            };

            mediaStreamSource.SampleRequested += (sender, args) =>
            {
                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        args.Request.Sample = null;
                        return;
                    }


                    var result = _grabber.GetScreenGrab(captureArea, true);

                    while (!result.IsSuccess || result.Value is null)
                    {
                        result = _grabber.GetScreenGrab(captureArea, true);
                    }

                    using var currentFrame = result.Value;
                    currentFrame.RotateFlip(RotateFlipType.RotateNoneFlipY);

                    var bd = currentFrame.LockBits(new Rectangle(Point.Empty, currentFrame.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    Marshal.Copy(bd.Scan0, tempBuffer, 0, size);
                    args.Request.Sample = MediaStreamSample.CreateFromBuffer(tempBuffer.AsBuffer(), stopwatch.Elapsed);
                    currentFrame.UnlockBits(bd);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while getting sample.");
                }
            };

            var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
            if (makeStreamable)
            {
                encodingProfile.Container.Subtype = MFTranscodeContainerType_FMPEG4.ToString("D");
            }
            encodingProfile.Video.Width = (uint)captureArea.Width;
            encodingProfile.Video.Height = (uint)captureArea.Height;
            // Default 15_000_000.
            encodingProfile.Video.Bitrate = 2_000_000;
            encodingProfile.Video.FrameRate.Numerator = (uint)frameRate;
           
            var transcoder = new MediaTranscoder
            {
                HardwareAccelerationEnabled = true,
                AlwaysReencode = true,
                VideoProcessingAlgorithm = MediaVideoProcessingAlgorithm.Default
            };

            var prepareResult = await transcoder.PrepareMediaStreamSourceTranscodeAsync(
                mediaStreamSource,
                destinationStream.AsRandomAccessStream(),
                encodingProfile);

            await prepareResult.TranscodeAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ex);
        }
    }
}
