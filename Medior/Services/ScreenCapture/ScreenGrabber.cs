using Medior.Models;
using Medior.Shared;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using PInvoke;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Medior.Services.ScreenCapture
{
    public interface IScreenGrabber
    {
        IEnumerable<DisplayInfo> GetDisplays();

        Result<Bitmap> GetScreenGrab(string outputName);
        Result<Bitmap> GetScreenGrab();
    }
    public class ScreenGrabber : IScreenGrabber
    {
        private readonly List<DisplayInfo> _displays = new();
        private readonly Dictionary<string, DxOutput> _dxOutputs = new();
        private readonly ILogger<ScreenGrabber> _logger;
       
        public ScreenGrabber(ILogger<ScreenGrabber> logger)
        {
            _logger = logger;
            InitDirectX();
        }

        public IEnumerable<DisplayInfo> GetDisplays()
        {
            if (!_displays.Any())
            {
                var displays = DisplaysEnumerationHelper.GetDisplays();
                _displays.AddRange(displays);
            }
            return _displays.ToArray();
        }

        public Result<Bitmap> GetScreenGrab(string outputName)
        {
            try
            {
                var display = GetDisplays().FirstOrDefault(x => x.DeviceName == outputName);

                if (display is null)
                {
                    return Result.Fail<Bitmap>("Display name not found.");
                }

                var result = GetDirectXGrab(display);

                if (!result.IsSuccess || result.Value is null || IsEmpty(result.Value))
                {
                    result = GetBitBltGrab(display.MonitorArea);
                    if (!result.IsSuccess || result.Value is null)
                    {
                        return Result.Fail<Bitmap>(result.Exception ?? new("Unknown error."));
                    }
                }

                return Result.Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grabbing screen.");
                return Result.Fail<Bitmap>(ex);
            }
        }

        public Result<Bitmap> GetScreenGrab()
        {
            try
            {
                var result = GetBitBltGrab(SystemInformation.VirtualScreen);
                if (!result.IsSuccess || result.Value is null)
                {
                    return Result.Fail<Bitmap>(result.Exception ?? new("Unknown error."));
                }

                return Result.Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grabbing screen.");
                return Result.Fail<Bitmap>(ex);
            }
        }

        internal Result<Bitmap> GetBitBltGrab(Rectangle captureArea)
        {
            var hwnd = IntPtr.Zero;
            var screenDc = new User32.SafeDCHandle();
            try
            {
                hwnd = User32.GetDesktopWindow();
                screenDc = User32.GetWindowDC(hwnd);
                var bitmap = new Bitmap(captureArea.Width, captureArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                var targetDc = graphics.GetHdc();
                var safeTargetDc = new User32.SafeDCHandle(hwnd, targetDc);
                
                Gdi32.BitBlt(safeTargetDc, 0, 0, bitmap.Width, bitmap.Height,
                    screenDc, captureArea.X, captureArea.Y, unchecked((int)CopyPixelOperation.SourceCopy));

                graphics.ReleaseHdc(targetDc);

                return Result.Ok(bitmap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grabbing with BitBlt.");
                return Result.Fail<Bitmap>(ex);
            }
            finally
            {
                _ = User32.ReleaseDC(hwnd, screenDc.HWnd);
            }
        }

        internal Result<Bitmap> GetDirectXGrab(DisplayInfo display)
        {
            if (!_dxOutputs.TryGetValue(display.DeviceName, out var dxOutput))
            {
                return Result.Fail<Bitmap>("DirectX output not found.");
            }


            try
            {
                var outputDuplication = dxOutput.OutputDuplication;
                var device = dxOutput.Device;
                var texture2D = dxOutput.Texture2D;
                var bounds = dxOutput.Bounds;

                var result = outputDuplication.TryAcquireNextFrame(50, out var duplicateFrameInfo, out var screenResource);

                if (!result.Success)
                {
                    return Result.Fail<Bitmap>("Next frame did not arrive.");
                }

                if (duplicateFrameInfo.AccumulatedFrames == 0)
                {
                    try
                    {
                        outputDuplication.ReleaseFrame();
                    }
                    catch { }
                    return Result.Fail<Bitmap>("No frames were accumulated.");
                }

                using Texture2D screenTexture2D = screenResource.QueryInterface<Texture2D>();
                device.ImmediateContext.CopyResource(screenTexture2D, texture2D);
                var dataBox = device.ImmediateContext.MapSubresource(texture2D, 0, MapMode.Read, MapFlags.None);
                var bitmap = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                var bitmapData = bitmap.LockBits(bounds, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                var dataBoxPointer = dataBox.DataPointer;
                var bitmapDataPointer = bitmapData.Scan0;
                for (var y = 0; y < bounds.Height; y++)
                {
                    SharpDX.Utilities.CopyMemory(bitmapDataPointer, dataBoxPointer, bounds.Width * 4);
                    dataBoxPointer = IntPtr.Add(dataBoxPointer, dataBox.RowPitch);
                    bitmapDataPointer = IntPtr.Add(bitmapDataPointer, bitmapData.Stride);
                }
                bitmap.UnlockBits(bitmapData);
                device.ImmediateContext.UnmapSubresource(texture2D, 0);
                screenResource?.Dispose();

                switch (dxOutput.Rotation)
                {
                    case DisplayModeRotation.Unspecified:
                    case DisplayModeRotation.Identity:
                        break;
                    case DisplayModeRotation.Rotate90:
                        bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                    case DisplayModeRotation.Rotate180:
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case DisplayModeRotation.Rotate270:
                        bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    default:
                        break;
                }
                return Result.Ok(bitmap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while grabbing with DirectX.");
            }
            finally
            {
                try
                {
                    dxOutput.OutputDuplication.ReleaseFrame();
                }
                catch { }
            }

            return Result.Fail<Bitmap>("Failed to get DirectX grab.");
        }

        internal Result<Bitmap> GetWinFormsGrab(DisplayInfo display)
        {
            try
            {
                var bitmap = new Bitmap(display.MonitorArea.Width, display.MonitorArea.Height);
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(Point.Empty, Point.Empty, bitmap.Size);
                return Result.Ok(bitmap);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grabbing with WinForms.");
                return Result.Fail<Bitmap>(ex);
            }
        }

        private void DisposeDxOutputs()
        {
            foreach (var screen in _dxOutputs.Values)
            {
                try
                {
                    screen.Dispose();
                }
                catch { }
            }
            _dxOutputs.Clear();
        }

        private void InitDirectX()
        {
            try
            {
                DisposeDxOutputs();

                using var factory = new Factory1();
                foreach (var adapter in factory.Adapters1.Where(x => (x.Outputs?.Length ?? 0) > 0))
                {
                    foreach (var output in adapter.Outputs)
                    {
                        try
                        {
                            var device = new Device(adapter);
                            var output1 = output.QueryInterface<Output1>();

                            var bounds = output1.Description.DesktopBounds;
                            var width = bounds.Right - bounds.Left;
                            var height = bounds.Bottom - bounds.Top;

                            // Create Staging texture CPU-accessible
                            var textureDesc = new Texture2DDescription
                            {
                                CpuAccessFlags = CpuAccessFlags.Read,
                                BindFlags = BindFlags.None,
                                Format = Format.B8G8R8A8_UNorm,
                                Width = width,
                                Height = height,
                                OptionFlags = ResourceOptionFlags.None,
                                MipLevels = 1,
                                ArraySize = 1,
                                SampleDescription = { Count = 1, Quality = 0 },
                                Usage = ResourceUsage.Staging
                            };

                            var texture2D = new Texture2D(device, textureDesc);

                            _dxOutputs.Add(
                                output1.Description.DeviceName,
                                new DxOutput(adapter,
                                    device,
                                    output1.DuplicateOutput(device),
                                    texture2D,
                                    output1.Description.Rotation));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error while building DX output.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while building DX outputs.");
            }
        }

        private bool IsEmpty(Bitmap bitmap)
        {
            var bounds = new Rectangle(Point.Empty, bitmap.Size);
            var height = bounds.Height;
            var width = bounds.Width;

            BitmapData bd = new();

            try
            {
                bd = bitmap.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                var bytesPerPixel = Image.GetPixelFormatSize(bitmap.PixelFormat) / 8;
                var totalSize = bd.Height * bd.Width * bytesPerPixel;

                unsafe
                {
                    byte* scan = (byte*)bd.Scan0.ToPointer();

                    for (var row = 0; row < height; row++)
                    {
                        for (var column = 0; column < width; column++)
                        {
                            var index = row * width * bytesPerPixel + column * bytesPerPixel;

                            byte* data = scan + index;

                            if (data[0] == 0 &&
                                data[1] == 0 &&
                                data[2] == 0 &&
                                data[3] == 0)
                            {
                                continue;
                            }

                            return false;
                        }
                    }

                    return true;
                }
            }
            catch
            {
                return true;
            }
            finally
            {
                try
                {
                    bitmap.UnlockBits(bd);
                }
                catch { }
            }
        }
    }
}
