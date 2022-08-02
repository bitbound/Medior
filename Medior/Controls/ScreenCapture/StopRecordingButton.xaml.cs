using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
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
            StopButton.Opacity = 1;
        }

        private void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new StopRecordingRequested());
        }
    }
}
