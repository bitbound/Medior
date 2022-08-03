using MahApps.Metro.Controls;
using Medior.ViewModels;
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

namespace Medior.Views
{
    /// <summary>
    /// Interaction logic for PhotoSorterView.xaml
    /// </summary>
    public partial class PhotoSorterView : UserControl
    {
        private IPhotoSorterViewModel? ViewModel => DataContext as IPhotoSorterViewModel;

        public PhotoSorterView()
        {
            InitializeComponent();
        }

        private void DestinationFileTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ViewModel?.NotifyCommandsCanExecuteChanged();
        }

        private void SortJobComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ViewModel?.NotifyCommandsCanExecuteChanged();
        }

        private void VariableHelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (WpfApp.Current.MainWindow is MetroWindow metroWindow)
            {
                var variablesFlyout = metroWindow.Flyouts.FindChild<Flyout>("VariablesFlyout");
                if (variablesFlyout is null)
                {
                    return;
                }
                variablesFlyout.IsOpen = true;
            }
        }
    }
}
