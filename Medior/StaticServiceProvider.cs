using Medior.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Medior.Services;
using MahApps.Metro.IconPacks;
using Medior.Extensions;
using Medior.Views;
using Medior.Services.ScreenCapture;
using Medior.Services.Messaging;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace Medior
{
    internal static class StaticServiceProvider
    {
        private static readonly SemaphoreSlim _buildLock = new(1, 1);
        private static IServiceProvider? _instance;
        public static IServiceProvider Instance => _instance ??= BuildProvider();

        private static IServiceProvider BuildProvider()
        {
            try
            {
                _buildLock.Wait();

                if (_instance is not null)
                {
                    return _instance;
                }

                var services = new ServiceCollection();

                services.AddSingleton<IShellViewModel, ShellViewModel>();
                services.AddSingleton<ISettingsViewModel, SettingsViewModel>();
                services.AddSingleton<IAboutViewModel, AboutViewModel>();

                services.AddAppModule<HomeView>("Home", PackIconOcticonsKind.Home);
                services.AddAppModule<IScreenshotViewModel, ScreenshotViewModel, ScreenshotView>("Screenshot", PackIconOcticonsKind.DeviceCamera);

                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<ISystemTime, SystemTime>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddSingleton<ILoaderService, LoaderService>();
                services.AddSingleton(services => DialogCoordinator.Instance);
                services.AddSingleton<ISettings, Settings>();
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                services.AddSingleton<IThemeSetter, ThemeSetter>();
                services.AddScoped<ICapturePicker, CapturePicker>();
                services.AddScoped<IScreenGrabber, ScreenGrabber>();
                services.AddHttpClient();
                services.AddLogging(builder => builder.AddProvider(new FileLoggerProvider()));

                _instance = services.BuildServiceProvider();

                _instance.GetRequiredService<ILoggerFactory>().AddProvider(new FileLoggerProvider());

                return _instance;
            }
            finally
            {
                _buildLock.Release();
            }
        }
    }
}
