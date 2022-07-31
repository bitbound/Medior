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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Hyperlink hyperlink)
            {
                hyperlink.LaunchUrl();
            }
        }
    }
}
