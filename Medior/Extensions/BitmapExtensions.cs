using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Medior.Extensions
{
    internal static class BitmapExtensions
    {
        private static readonly RecyclableMemoryStreamManager _streamManager = new();
        public static BitmapImage ToBitmapImage(this Bitmap bitmap, ImageFormat imageFormat)
        {
            using var ms = _streamManager.GetStream();
            bitmap.Save(ms, imageFormat);
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        public static ImageBrush ToImageBrush(this Bitmap bitmap, ImageFormat imageFormat)
        {
            var bitmapImage = bitmap.ToBitmapImage(imageFormat);
            return new ImageBrush(bitmapImage);
        }
    }
}
