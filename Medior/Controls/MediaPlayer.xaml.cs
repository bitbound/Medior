using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for MediaPlayer.xaml
    /// </summary>
    public partial class MediaPlayer : UserControl
    {
        // Using a DependencyProperty as the backing store for MediaUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaUriProperty =
            DependencyProperty.Register("MediaUri", typeof(Uri), typeof(FrameworkElement), new PropertyMetadata());

        private bool _isPlaying;

        public MediaPlayer()
        {
            InitializeComponent();
        }

        public Uri MediaUri
        {
            get { return (Uri)GetValue(MediaUriProperty); }
            set { SetValue(MediaUriProperty, value); }
        }

        private bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                if (value)
                {
                    PlayButton.Visibility = Visibility.Collapsed;
                    PauseButton.Visibility = Visibility.Visible;
                    MediaViewer.Play();
                    _ = Task.Run(SyncTimelineSlider);
                }
                else
                {
                    PlayButton.Visibility = Visibility.Visible;
                    PauseButton.Visibility = Visibility.Collapsed;
                    MediaViewer.Stop();
                }
            }
        }
        private void MediaViewer_MediaEnded(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;
        }

        private void MediaViewer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            IsPlaying = false;
        }

        private void MediaViewer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (MediaViewer.NaturalDuration.HasTimeSpan)
            {
                TimelineSlider.Maximum = MediaViewer.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
        }

        private void MediaViewer_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            IsPlaying = true;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
        }

        private async Task SyncTimelineSlider()
        {
            while (IsPlaying)
            {
                Dispatcher.Invoke(() =>
                {
                    if (MediaViewer.NaturalDuration.HasTimeSpan)
                    {
                        TimelineSlider.Value = MediaViewer.Position.TotalMilliseconds;
                    }
                });
                await Task.Delay(500);
            }
        }
        private void TimelineSlider_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsPlaying = false;
        }

        private void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaViewer.Position = new TimeSpan(0, 0, 0, 0, (int)e.NewValue);
        }

        private void MediaViewer_Loaded(object sender, RoutedEventArgs e)
        {
            IsPlaying = true;
        }
    }
}
