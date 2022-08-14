using Medior.Native;
using Medior.Shared.Helpers;
using PInvoke;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Rectangle = System.Drawing.Rectangle;

namespace Medior.Controls.ScreenCapture
{
    /// <summary>
    /// Interaction logic for CapturePickerWindow.xaml
    /// </summary>
    public partial class CapturePickerWindow : Window
    {
        public Bitmap? _backgroundImage;

        private double _dpiScale = 1;

        private System.Windows.Point _startPoint;

        public CapturePickerWindow()
        {
            InitializeComponent();
        }

        public CapturePickerWindow(Bitmap backgroundImage)
        {
            InitializeComponent();
            _backgroundImage = backgroundImage;
        }

        public Rect Rect => new(0, 0, ActualWidth, ActualHeight);

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
                    (int)Math.Max(0, CaptureBorder.Bounds.Left * _dpiScale + left),
                    (int)Math.Max(0, CaptureBorder.Bounds.Top * _dpiScale + top),
                    (int)Math.Min(width, CaptureBorder.Bounds.Width * _dpiScale),
                    (int)Math.Min(height, CaptureBorder.Bounds.Height * _dpiScale)),

                false => new Rectangle(
                    (int)Math.Max(0, CaptureBorder.Bounds.Left + left),
                    (int)Math.Max(0, CaptureBorder.Bounds.Top + top),
                    (int)Math.Min(width, CaptureBorder.Bounds.Width),
                    (int)Math.Min(height, CaptureBorder.Bounds.Height))
            };
        }

        private async Task FrameWindowUnderCursor()
        {
            var screen = SystemInformation.VirtualScreen;
            var thisHandle = new WindowInteropHelper(this).Handle;
            var shellHandle = User32.GetShellWindow();
            var desktopHandle = User32.GetDesktopWindow();

            while (IsVisible)
            {
                if (!await WaitHelper.WaitForAsync(() => IsCtrlPressed(), TimeSpan.FromMilliseconds(100)))
                {
                    continue;
                }

                var (targetRect, targetHwnd) = WindowHelpers.GetWindowUnderCursor(thisHandle, shellHandle, desktopHandle);

                var (childRect, childHwnd) = WindowHelpers.GetChildWindow(targetHwnd);

                if (!childRect.IsEmpty())
                {
                    targetRect = childRect;
                    targetHwnd = childHwnd;
                }

                CaptureBorder.Rect = new Rect(
                    (targetRect.left - screen.Left) * _dpiScale,
                    (targetRect.top - screen.Top) * _dpiScale,
                    targetRect.Width() * _dpiScale,
                    targetRect.Height() * _dpiScale);

                await Task.Delay(100);
            }
        }

        private bool IsCtrlPressed()
        {
            return 
                Keyboard.GetKeyStates(Key.LeftCtrl).HasFlag(KeyStates.Down) ||
                Keyboard.GetKeyStates(Key.RightCtrl).HasFlag(KeyStates.Down);
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

            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;

            MainGrid.Background = _backgroundImage?.ToImageBrush(ImageFormat.Png);

            Activate();
            HeaderHint.Visibility = Visibility.Visible;

            _ = FrameWindowUnderCursor();
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
            if (e.LeftButton == MouseButtonState.Pressed && !IsCtrlPressed())
            {
                var pos = e.GetPosition(this);
                var left = Math.Min(pos.X, _startPoint.X);
                var top = Math.Min(pos.Y, _startPoint.Y);
                var width = Math.Abs(_startPoint.X - pos.X);
                var height = Math.Abs(_startPoint.Y - pos.Y);
                CaptureBorder.Rect = new Rect(left, top, width, height);
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
