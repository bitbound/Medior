using Medior.Controls;
using Medior.Controls.ScreenCapture;
using Medior.Shared.Helpers;
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
        void ShowQrCode(string url, string windowTitle);

        IDisposable ShowRecordingFrame(Rectangle selectedArea);
        IDisposable ShowRemoteControlWindow(Guid streamId);
    }

    public class WindowService : IWindowService
    {
        private readonly IMessenger _messenger;
        private readonly IUiDispatcher _uiDispatcher;
        private readonly IEnvironmentHelper _envHelper;
        private readonly IQrCodeGenerator _qrCodeGenerator;
        public WindowService(
            IQrCodeGenerator qrCodeGenerator, 
            IMessenger messenger,
            IUiDispatcher uiDispatcher,
            IEnvironmentHelper envHelper)
        {
            _qrCodeGenerator = qrCodeGenerator;
            _messenger = messenger;
            _uiDispatcher = uiDispatcher;
            _envHelper = envHelper;
        }

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
            if (_envHelper.IsDebug)
            {
                window.Topmost = false;
            }
            window.ShowDialog();
            return window.SelectedArea;
        }
        public Rectangle ShowCapturePicker()
        {
            var window = new CapturePickerWindow();
            if (_envHelper.IsDebug)
            {
                window.Topmost = false;
            }
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

        public void ShowQrCode(string url, string windowTitle)
        {
            var result = _qrCodeGenerator.GenerateCode(url);
            if (!result.IsSuccess)
            {
                _messenger.Send(new ToastMessage("Failed to generate QR code", ToastType.Error));
            }

            var window = new QrCodeWindow(result.Value, windowTitle, url);
            window.Show();
        }

        public IDisposable ShowRecordingFrame(Rectangle selectedArea)
        {
            var frame = new RecordingFrameWindow(selectedArea);
            var stopButton = new StopRecordingButton();
            frame.Show();
            stopButton.Show();
            return new CallbackDisposable(() =>
            {
                _uiDispatcher.Invoke(() =>
                {
                    frame.Close();
                    stopButton.Close();
                });
            });
        }

        public IDisposable ShowRemoteControlWindow(Guid streamId)
        {
            var window = new RemoteControlWindow(streamId);
            window.Show();
            return new CallbackDisposable(() =>
            {
                window.Close();
            });
        }
    }
}
