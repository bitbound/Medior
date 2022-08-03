﻿using Microsoft.Toolkit.Mvvm.Input;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Medior.ViewModels
{
    public interface IAboutViewModel
    {
        string Version { get; }
        ICommand OpenLogsFolder { get; }
    }

    public class AboutViewModel : ObservableObjectEx, IAboutViewModel
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
