using System;
using System.Windows;
using System.Windows.Forms;

namespace Medior.Controls.ScreenCapture
{
    /// <summary>
    /// Interaction logic for StopRecordingButton.xaml
    /// </summary>
    public partial class StopRecordingButton : Window
    {
        public StopRecordingButton()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = Screen.PrimaryScreen.WorkingArea.Right - Width;
            Top = Screen.PrimaryScreen.WorkingArea.Bottom - Height;

            foreach (var window in WpfApp.Current.Windows)
            {
                if (window is RecordingFrameWindow frame)
                {
                    var maxLeft = SystemInformation.VirtualScreen.Right - Width;
                    var maxTop = SystemInformation.VirtualScreen.Bottom - Height;

                    Left = Math.Min(maxLeft, frame.Left + frame.Width - Width);
                    Top = Math.Min(maxTop, frame.Top + frame.Height);
                }
            }
            StopButton.Opacity = 1;
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new StopRecordingRequested());
        }
    }
}
