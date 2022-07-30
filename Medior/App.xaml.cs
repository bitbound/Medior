using Medior.Interfaces;
using Medior.Native;
using Medior.Services;
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

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var appStartup = StaticServiceProvider.Instance.GetRequiredService<IAppStartup>();
            appStartup.Initialize(_cts.Token);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _cts.Cancel();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show("There was an unhandled error.\n\n" +
                $"Error: {e.Exception.Message}\n\n" +
                $"Stack Trace: {e.Exception.StackTrace}",
                "Unhandled Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
