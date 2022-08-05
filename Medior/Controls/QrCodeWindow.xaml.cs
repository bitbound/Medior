using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
