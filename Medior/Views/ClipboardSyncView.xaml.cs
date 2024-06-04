using Medior.Shared.Dtos;
using Medior.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Medior.Views;

/// <summary>
/// Interaction logic for ClipboardSyncView.xaml
/// </summary>
public partial class ClipboardSyncView : UserControl
{
    public ClipboardSyncView()
    {
        InitializeComponent();
    }

    public ClipboardSyncViewModel? ViewModel => DataContext as ClipboardSyncViewModel;

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel?.RefreshClips();
    }

    private async void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (ViewModel is null || e.EditAction == DataGridEditAction.Cancel)
        {
            return;
        }

        if (e.EditingElement.DataContext is ClipboardSaveDto dto)
        {
            await ViewModel.UpdateClipboardSave(dto);
        }
    }

}
