using Medior.Controls;
using Medior.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Medior.Services
{
    internal interface IWindowService
    {
        Rectangle ShowCapturePicker(Bitmap backgroundImage);
        IDisposable HideMainWindow();
        void ShowMainWindow();
    }

    internal class WindowService : IWindowService
    {
        public Rectangle ShowCapturePicker(Bitmap backgroundImage)
        {
            var window = new CapturePickerWindow(backgroundImage);
            window.ShowDialog();
            return window.SelectedArea;
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

        public void ShowMainWindow()
        {
            try
            {
                if (WpfApp.Current.MainWindow is null)
                {
                    WpfApp.Current.MainWindow = new ShellWindow();
                }

                WpfApp.Current.MainWindow.Show();
                WpfApp.Current.MainWindow.Activate();
            }
            catch { }
        }
    }
}
