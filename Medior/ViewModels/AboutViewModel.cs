using Medior.Reactive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Medior.ViewModels
{
    internal interface IAboutViewModel
    {
        string Version { get; }
        ICommand OpenLogsFolder { get; }
    }

    internal class AboutViewModel : ObservableObject, IAboutViewModel
    {
        public AboutViewModel()
        {
            OpenLogsFolder = new RelayCommand(() =>
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = AppConstants.LogsFolderPath
                };
                Process.Start(psi);
            });
        }
        public string Version { get; } = $"{Assembly.GetExecutingAssembly().GetName().Version}";

        public ICommand OpenLogsFolder { get; }
    }
}
