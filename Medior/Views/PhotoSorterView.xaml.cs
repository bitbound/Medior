using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Medior.Views;

/// <summary>
/// Interaction logic for PhotoSorterView.xaml
/// </summary>
public partial class PhotoSorterView : UserControl
{
    private PhotoSorterViewModel? ViewModel => DataContext as PhotoSorterViewModel;

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
        var messenger = StaticServiceProvider.Instance.GetRequiredService<IMessenger>();
        messenger.SendGenericMessage(FlyoutRequestKind.PhotoSorterDestinationVariables);
    }
}
