using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class AboutViewModel
    {
        private readonly IProcessService _processService;

        public AboutViewModel(IProcessService processService)
        {
            _processService = processService;
        }

        public string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version}";

        [RelayCommand]
        private void OpenLogsFolder()
        {
            var psi = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = AppConstants.LogsFolderPath
            };
            _processService.Start(psi);
        }
    }
}
