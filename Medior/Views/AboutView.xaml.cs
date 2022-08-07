using Medior.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Medior.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();
        }

        public AboutViewModel? ViewModel => DataContext as AboutViewModel;

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink)
            {
                hyperlink.LaunchUrl();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.RefreshUpdateStatus();
        }
    }
}
