﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
    /// Interaction logic for CapturePickerWindow.xaml
    /// </summary>
    public partial class CapturePickerWindow : Window
    {
        public CapturePickerWindow()
        {
            InitializeComponent();
        }

        public System.Drawing.Rectangle SelectedArea { get; private set; }

        public Bitmap? BackgroundImage { get; set; }
        private System.Windows.Point StartPoint { get; set; }
        private double DpiScale { get; set; } = 1;
        private bool ManualRegionSelection { get; set; } = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DpiScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            labelHeader.Visibility = Visibility.Visible;
            CaptureBorder.Visibility = Visibility.Visible;
            this.Width = SystemParameters.VirtualScreenWidth;
            this.Height = SystemParameters.VirtualScreenHeight;
            this.Left = SystemParameters.VirtualScreenLeft;
            this.Top = SystemParameters.VirtualScreenTop;

            using var ms = new MemoryStream();
            BackgroundImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = ms;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            MainGrid.Background = new ImageBrush(bitmap);
            FrameWindowUnderCursor();
            this.Activate();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                if (textConfirmRegion.IsVisible)
                {
                    textConfirmRegion.Visibility = Visibility.Collapsed;
                }
                StartPoint = e.GetPosition(this);
            }

        }

        private async void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (textConfirmRegion.IsVisible)
                {
                    if (captureMode == Models.CaptureMode.PNG)
                    {
                        try
                        {
                            await HideAllButBackground();
                            Screenshot.SaveCapture(Screenshot.GetCapture(GetDrawnRegion(true), false));
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("There was an error capturing the screenshot.  If the issue persists, please contact translucency@outlook.com.", "Capture Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            MainWindow.Current.WriteToLog(ex);
                            this.Close();
                        }
                    }
                    else if (captureMode == Models.CaptureMode.GIF)
                    {
                        CaptureControls controls = null;
                        try
                        {
                            await HideAllButBackground();
                            var region = GetDrawnRegion(false);
                            await CaptureControls.Create(region);
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            controls?.Close();
                            System.Windows.MessageBox.Show("There was an error recording the GIF.  If the issue persists, please contact translucency@outlook.com.", "Capture Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            MainWindow.Current.WriteToLog(ex);
                            this.Close();
                        }

                    }
                }
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                ManualRegionSelection = true;
                textConfirmRegion.Visibility = Visibility.Visible;
            }
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ManualRegionSelection = true;
                var pos = e.GetPosition(this);
                var left = Math.Min(pos.X, StartPoint.X);
                var top = Math.Min(pos.Y, StartPoint.Y);
                var width = Math.Abs(StartPoint.X - pos.X);
                var height = Math.Abs(StartPoint.Y - pos.Y);
                borderCapture.Margin = new Thickness(left, top, 0, 0);
                borderCapture.Width = width;
                borderCapture.Height = height;
            }
        }
        public Rect GetDrawnRegion(bool scaleWithDPI)
        {
            if (scaleWithDPI)
            {
                return new Rect(Math.Round(borderCapture.Margin.Left * DpiScale + SystemInformation.VirtualScreen.Left, 0),
                    Math.Round(borderCapture.Margin.Top * DpiScale + SystemInformation.VirtualScreen.Top, 0),
                    Math.Round(borderCapture.Width * DpiScale, 0),
                    Math.Round(borderCapture.Height * DpiScale, 0));
            }
            else
            {
                return new Rect(Math.Round(borderCapture.Margin.Left + SystemInformation.VirtualScreen.Left, 0),
                    Math.Round(borderCapture.Margin.Top + SystemInformation.VirtualScreen.Top, 0),
                    Math.Round(borderCapture.Width, 0),
                    Math.Round(borderCapture.Height, 0));
            }
        }
        private async Task HideAllButBackground()
        {
            textConfirmRegion.Visibility = Visibility.Collapsed;
            labelHeader.Visibility = Visibility.Collapsed;
            borderCapture.Visibility = Visibility.Collapsed;
            do
            {
                await Task.Delay(100);
            }
            while (labelHeader.IsVisible || textConfirmRegion.IsVisible || borderCapture.IsVisible);
        }
        private async void FrameWindowUnderCursor()
        {
            var point = new System.Drawing.Point();
            var rect = new User32.RECT();
            var screen = SystemInformation.VirtualScreen;
            var winList = new List<IntPtr>();
            var thisHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            var shellHandle = User32.GetShellWindow();
            var desktopHandle = User32.GetDesktopWindow();
            while (this.IsVisible && ManualRegionSelection == false)
            {
                User32.GetCursorPos(out point);
                winList.Clear();
                var devenv = System.Diagnostics.Process.GetProcesses().FirstOrDefault(proc => proc.MainWindowTitle.Contains("CleanShot") && proc.MainWindowTitle.Contains("Microsoft"));
                User32.EnumWindows((hWin, lParam) => {
                    if (hWin == thisHandle || hWin == shellHandle || hWin == desktopHandle || !User32.IsWindowVisible(hWin))
                    {
                        return true;
                    }
                    User32.GetWindowRect(hWin, out rect);
                    if (rect.Width == screen.Width && rect.Height == screen.Height)
                    {
                        return true;
                    }
                    if (rect.Left < point.X && rect.Top < point.Y && rect.Right > point.X && rect.Bottom > point.Y)
                    {
                        winList.Add(hWin);
                    }
                    return true;
                }, IntPtr.Zero);
                if (winList.Count > 0)
                {

                    User32.GetWindowRect(winList.First(), out rect);
                }
                else
                {
                    User32.GetWindowRect(shellHandle, out rect);
                }
                borderCapture.Margin = new Thickness(rect.Left - screen.Left, rect.Top - screen.Top, 0, 0);
                borderCapture.Width = rect.Width;
                borderCapture.Height = rect.Height;
                await Task.Delay(100);
            }
        }


    }
}
