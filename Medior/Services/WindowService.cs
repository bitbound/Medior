using Medior.Controls.ScreenCapture;
using Medior.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Medior.Services
{
    public interface IWindowService
    {
        IDisposable HideMainWindow();

        Rectangle ShowCapturePicker(Bitmap backgroundImage);
        Rectangle ShowCapturePicker();
        void ShowMainWindow();
        IDisposable ShowRecordingFrame(Rectangle selectedArea);
    }

    public class WindowService : IWindowService
    {
        public IDisposable HideMainWindow()
        {
            try
            {
                if (WpfApp.Current.MainWindow is null)
                {
                    return CallbackDisposable.Empty;
                }

                var startLeft = WpfApp.Current.MainWindow.Left;
                var startTop = WpfApp.Current.MainWindow.Top;
                WpfApp.Current.MainWindow.Left = SystemInformation.VirtualScreen.Right * 2;
                WpfApp.Current.MainWindow.Top = SystemInformation.VirtualScreen.Bottom * 2;

                return new CallbackDisposable(() =>
                {
                    try
                    {
                        WpfApp.Current.MainWindow.Left = startLeft;
                        WpfApp.Current.MainWindow.Top = startTop;
                    }
                    catch { }
                });
            }
            catch
            {
                return CallbackDisposable.Empty;
            }
        }

        public Rectangle ShowCapturePicker(Bitmap backgroundImage)
        {
            var window = new CapturePickerWindow(backgroundImage);
            window.ShowDialog();
            return window.SelectedArea;
        }
        public Rectangle ShowCapturePicker()
        {
            var window = new CapturePickerWindow();
            window.ShowDialog();
            return window.SelectedArea;
        }

        public void ShowMainWindow()
        {
            try
            {
                if (WpfApp.Current.MainWindow is null)
                {
                    WpfApp.Current.MainWindow = new ShellWindow();
                }

                WpfApp.Current.MainWindow.Show();
                if (WpfApp.Current.MainWindow.WindowState == System.Windows.WindowState.Minimized)
                {
                    WpfApp.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                }
                WpfApp.Current.MainWindow.Activate();
            }
            catch { }
        }

        public IDisposable ShowRecordingFrame(Rectangle selectedArea)
        {
            var frame = new RecordingFrameWindow(selectedArea);
            var stopButton = new StopRecordingButton();
            frame.Show();
            stopButton.Show();
            return new CallbackDisposable(() =>
            {
                frame.Close();
                stopButton.Close();
            });
        }
    }
}
