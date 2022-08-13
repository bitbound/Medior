using Medior.Shared.Dtos;
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
}
