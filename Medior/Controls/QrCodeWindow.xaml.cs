using MahApps.Metro.Controls;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for QrCodeWindow.xaml
    /// </summary>
    public partial class QrCodeWindow : MetroWindow
    {
        private readonly Bitmap? _qrCode;
        private readonly string _title = string.Empty;

        public QrCodeWindow()
        {
            InitializeComponent();
        }

        public QrCodeWindow(Bitmap? value, string title)
        {
            InitializeComponent();
            _qrCode = value;
            _title = title;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = _title;
            HeaderText.Text = _title;
            QrImage.Source = _qrCode?.ToBitmapImage(ImageFormat.Png);
        }
    }
}
