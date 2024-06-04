using Medior.ViewModels;
using Microsoft.Win32;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Views;

/// <summary>
/// Interaction logic for FileSharingView.xaml
/// </summary>
public partial class FileSharingView : UserControl
{
    public FileSharingView()
    {
        InitializeComponent();
    }

    public FileSharingViewModel? ViewModel => DataContext as FileSharingViewModel;

    private void UploadsGrid_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.Copy;
    }

    private async void UploadsGrid_Drop(object sender, DragEventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        e.Handled = true;

        if (!e.Data.GetDataPresent("FileDrop"))
        {
            return;
        }

        if (e.Data.GetData("FileDrop") is not string[] fileList)
        {
            return;
        }

        await ViewModel.UploadFiles(fileList);
    }

    private void UploadButton_Click(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            Multiselect = true
        };
        ofd.ShowDialog();

        if (ofd.FileNames?.Any() == true)
        {
            ViewModel?.UploadFiles(ofd.FileNames);
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel?.RefreshUploads();
    }
}
