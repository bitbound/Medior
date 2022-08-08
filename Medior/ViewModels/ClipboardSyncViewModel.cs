using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class ClipboardSyncViewModel
    {
        [ObservableProperty]
        private ImageSource? _qrCodeImage;

        [ObservableProperty]
        private string _syncUrl = string.Empty;

        [RelayCommand]
        private void CopySyncUrl()
        {

        }
    }
}
