using MahApps.Metro.Controls.Dialogs;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
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

            if (!Environment.GetCommandLineArgs().Any(x => x.Equals("--hidden", StringComparison.OrdinalIgnoreCase)))
            {
                Current.MainWindow = new ShellWindow();
                Current.MainWindow.Show();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _cts.Cancel();
        }

        private async void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            var logger = StaticServiceProvider.Instance.GetRequiredService<ILogger<App>>();
            logger.LogError(e.Exception, "Unhandled exceptions.");

            await _dialogCoordinator.ShowMessageAsync(ViewModelLocator.ShellWindowViewModel,
              "Oh darn.  An error.",
              $"Here's what it said:\n\n{e.Exception.Message}",
              MessageDialogStyle.Affirmative);

        }
    }
}
