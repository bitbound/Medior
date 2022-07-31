using Medior.Shared;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace ScreenR.Desktop.Control.Services
{
    public interface IBitmapUtility
    {
        Bitmap CropBitmap(Bitmap bitmap, Rectangle cropArea);
        Result<Rectangle> GetDiffArea(Bitmap currentFrame, Bitmap? previousFrame, bool forceFullscreen = false);
    }

    public class BitmapUtility : IBitmapUtility
    {
        private readonly ILogger<BitmapUtility> _logger;

        public BitmapUtility(ILogger<BitmapUtility> logger)
        {
            _logger = logger;
        }

        public Bitmap CropBitmap(Bitmap bitmap, Rectangle cropArea)
        {
            return bitmap.Clone(cropArea, bitmap.PixelFormat);
        }

        public Result<Rectangle> GetDiffArea(Bitmap currentFrame, Bitmap? previousFrame, bool forceFullscreen = false)
        {
            if (currentFrame == null || previousFrame == null)
            {
                return Result.Ok(Rectangle.Empty);
            }

            if (forceFullscreen)
            {
                return Result.Ok(new Rectangle(new Point(0, 0), currentFrame.Size));
            }

            if (currentFrame.Height != previousFrame.Height || currentFrame.Width != previousFrame.Width)
            {
                return Result.Fail<Rectangle>("Bitmaps are not of equal dimensions.");
            }

            if (currentFrame.PixelFormat != previousFrame.PixelFormat)
            {
                return Result.Fail<Rectangle>("Bitmaps are not the same format.");
            }

            var width = currentFrame.Width;
            var height = currentFrame.Height;
            int left = int.MaxValue;
            int top = int.MaxValue;
            int right = int.MinValue;
            int bottom = int.MinValue;

            BitmapData bd1 = new();
            BitmapData bd2 = new();

            try
            {
                bd1 = previousFrame.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, currentFrame.PixelFormat);
                bd2 = currentFrame.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, previousFrame.PixelFormat);

                var bytesPerPixel = Bitmap.GetPixelFormatSize(currentFrame.PixelFormat) / 8;
                var totalSize = bd1.Height * bd1.Width * bytesPerPixel;

                unsafe
                {
                    byte* scan1 = (byte*)bd1.Scan0.ToPointer();
                    byte* scan2 = (byte*)bd2.Scan0.ToPointer();

                    for (var row = 0; row < height; row++)
                    {
                        for (var column = 0; column < width; column++)
                        {
                            var index = (row * width * bytesPerPixel) + (column * bytesPerPixel);

                            byte* data1 = scan1 + index;
                            byte* data2 = scan2 + index;

                            if (data1[0] != data2[0] ||
                                data1[1] != data2[1] ||
                                data1[2] != data2[2])
                            {

                                if (row < top)
                                {
                                    top = row;
                                }
                                if (row > bottom)
                                {
                                    bottom = row;
                                }
                                if (column < left)
                                {
                                    left = column;
                                }
                                if (column > right)
                                {
                                    right = column;
                                }
                            }

                        }
                    }

                    if (left <= right && top <= bottom)
                    {
                        left = Math.Max(left - 2, 0);
                        top = Math.Max(top - 2, 0);
                        right = Math.Min(right + 2, width);
                        bottom = Math.Min(bottom + 2, height);

                        return Result.Ok(new Rectangle(left, top, right - left, bottom - top));
                    }
                    else
                    {
                        return Result.Ok(Rectangle.Empty);
                    }
                }
            }
            catch
            {
                return Result.Ok(Rectangle.Empty);
            }
            finally
            {
                try
                {
                    currentFrame.UnlockBits(bd1);
                    previousFrame.UnlockBits(bd2);
                }
                catch { }
            }
        }
    }
}
