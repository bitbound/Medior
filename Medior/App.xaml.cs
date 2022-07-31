using MahApps.Metro.Controls.Dialogs;
using Medior.Interfaces;
using Medior.Native;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Medior
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly CancellationTokenSource _cts = new();
        private readonly DialogCoordinator _dialogCoordinator = new();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var appStartup = StaticServiceProvider.Instance.GetRequiredService<IAppStartup>();
            appStartup.Initialize(_cts.Token);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _cts.Cancel();
        }

        private async void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            await _dialogCoordinator.ShowMessageAsync(ViewModelLocator.ShellWindowViewModel,
              "Oh darn.  An error.",
              $"Here's what it said:\n\n{e.Exception.Message}",
              MessageDialogStyle.Affirmative);
        }
    }
}
