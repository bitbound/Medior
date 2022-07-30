using Medior.Extensions;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Rectangle = System.Drawing.Rectangle;

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for CapturePickerWindow.xaml
    /// </summary>
    public partial class CapturePickerWindow : Window
    {
        public Bitmap _backgroundImage;

        private double _dpiScale = 1;

        private System.Windows.Point _startPoint;


        public CapturePickerWindow()
        {
            InitializeComponent();
            _backgroundImage = new(0, 0);
        }

        public CapturePickerWindow(Bitmap backgroundImage)
        {
            InitializeComponent();
            _backgroundImage = backgroundImage;
        }

        public Rectangle SelectedArea { get; private set; }

        public Rectangle GetDrawnRegion(bool scaleWithDPI)
        {
            var left = SystemInformation.VirtualScreen.Left;
            var top = SystemInformation.VirtualScreen.Top;
            var width = SystemInformation.VirtualScreen.Width;
            var height = SystemInformation.VirtualScreen.Height;

            return scaleWithDPI switch
            {
                true => new Rectangle(
                    (int)Math.Max(0, CaptureBorder.Margin.Left * _dpiScale + left),
                    (int)Math.Max(0, CaptureBorder.Margin.Top * _dpiScale + top),
                    (int)Math.Min(width, CaptureBorder.Width * _dpiScale),
                    (int)Math.Min(height, CaptureBorder.Height * _dpiScale)),

                false => new Rectangle(
                    (int)Math.Max(0, CaptureBorder.Margin.Left + left),
                    (int)Math.Max(0, CaptureBorder.Margin.Top + top),
                    (int)Math.Min(width, CaptureBorder.Width),
                    (int)Math.Min(height, CaptureBorder.Height))
            };
        }

        private async Task FrameWindowUnderCursor()
        {
            var screen = SystemInformation.VirtualScreen;
            var thisHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var shellHandle = User32.GetShellWindow();
            var desktopHandle = User32.GetDesktopWindow();

            while (IsVisible && Mouse.LeftButton == MouseButtonState.Released)
            {
                var targetRect = new RECT();
                User32.GetCursorPos(out var point);

                User32.EnumWindows((hWin, lParam) =>
                {
                    if (hWin == thisHandle || hWin == shellHandle || hWin == desktopHandle || !User32.IsWindowVisible(hWin))
                    {
                        return true;
                    }

                    User32.GetWindowRect(hWin, out var rect);

                    if (rect.Width() == screen.Width && rect.Height() == screen.Height)
                    {
                        return true;
                    }

                    if (targetRect.IsEmpty() && point.IsOver(rect))
                    {
                        targetRect = rect;
                    }

                    return true;
                }, IntPtr.Zero);

                if (targetRect.IsEmpty())
                {
                    User32.GetWindowRect(shellHandle, out targetRect);
                }

                // TODO: Factor in DPI.
                CaptureBorder.Margin = new Thickness(targetRect.left - screen.Left, targetRect.top - screen.Top, 0, 0);
                CaptureBorder.Width = targetRect.Width();
                CaptureBorder.Height = targetRect.Height();
                await Task.Delay(100);
            }
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _dpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            HeaderLabel.Visibility = Visibility.Visible;
            CaptureBorder.Visibility = Visibility.Visible;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;

            MainGrid.Background = _backgroundImage.ToImageBrush(ImageFormat.Png);
            _ = FrameWindowUnderCursor();
            Activate();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                _startPoint = e.GetPosition(this);
            }

        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(this);
                var left = Math.Min(pos.X, _startPoint.X);
                var top = Math.Min(pos.Y, _startPoint.Y);
                var width = Math.Abs(_startPoint.X - pos.X);
                var height = Math.Abs(_startPoint.Y - pos.Y);
                CaptureBorder.Margin = new Thickness(left, top, 0, 0);
                CaptureBorder.Width = width;
                CaptureBorder.Height = height;
            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                SelectedArea = GetDrawnRegion(true);
                Close();
            }
        }
    }
}
