using System;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Medior.Services
{
    public interface ITrayService
    {
        Task ShowBalloon(string title, string message, int timeoutMs = 5_000, ToolTipIcon icon = ToolTipIcon.Info, Action? onclickCallback = null);
        void Initialize();
    }

    public class TrayService : ITrayService
    {
        private readonly SemaphoreSlim _balloonLock = new(1, 1);
        private NotifyIcon? _notifyIcon;

        public void Initialize()
        {
            if (_notifyIcon is not null)
            {
                return;
            }

            _notifyIcon = new NotifyIcon();
            _notifyIcon.DoubleClick += ShowMainWindow;
            _notifyIcon.ContextMenuStrip = new ContextMenuStrip();

            using var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Medior.Assets.comet-embedded.ico");
            if (iconStream is not null)
            {
                _notifyIcon.Icon = new Icon(iconStream);
            }

            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += ShowMainWindow;

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, a) => WpfApp.Current.Shutdown();

            _notifyIcon.ContextMenuStrip.Items.Add(openItem);
            _notifyIcon.ContextMenuStrip.Items.Add(exitItem);

            WpfApp.Current.Exit += (s, e) =>
            {
                _notifyIcon.Dispose();
            };

            _notifyIcon.Visible = true;
            
        }

        private void ShowMainWindow(object? sender, EventArgs e)
        {
            if (WpfApp.Current.MainWindow is null)
            {
                WpfApp.Current.MainWindow = new ShellWindow();
            }
            WpfApp.Current.MainWindow.Show();
        }

        public async Task ShowBalloon(string title, string message, int timeoutMs = 5_000, ToolTipIcon icon = ToolTipIcon.Info, Action? onclickCallback = null)
        {
            await _balloonLock.WaitAsync();

            Initialize();

            _notifyIcon!.ShowBalloonTip(timeoutMs, title, message, icon);

            if (onclickCallback is not null)
            {
                _notifyIcon.BalloonTipClosed += (s, e) =>
                {
                    try
                    {
                        _balloonLock.Release();
                    }
                    catch { }
                };
                void OnClick(object? sender, EventArgs e)
                {
                    onclickCallback.Invoke();
                    _notifyIcon.BalloonTipClicked -= OnClick;
                }
                _notifyIcon.BalloonTipClicked += OnClick;
            }
            else
            {
                _balloonLock.Release();
            }
        }
    }
}
