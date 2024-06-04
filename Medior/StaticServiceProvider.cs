using Medior.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using MahApps.Metro.IconPacks;
using Medior.Views;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Services.PhotoSorter;
using Views;
using Medior.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Medior.Shared.Services.Http;
using System.Net.Http;

namespace Medior;

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

            services.AddSingleton<IAppStartup, AppStartup>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ToastsViewModel>();
            services.AddSingleton<AboutViewModel>();

            services.AddAppModule<HomeView>("Home", PackIconBoxIconsKind.RegularHome);
            services.AddAppModule<ScreenCaptureViewModel, ScreenCaptureView>("Screen Capture", PackIconBoxIconsKind.RegularCamera);
            services.AddAppModule<PhotoSorterViewModel, PhotoSorterView>("Photo Sorter", PackIconBoxIconsKind.RegularPhotoAlbum);
            services.AddAppModule<KeepAwakeViewModel, KeepAwakeView>("Keep Awake", PackIconBoxIconsKind.RegularCoffee);
            services.AddAppModule<QrCodeGeneratorViewModel, QrCodeGeneratorView>("QR Code", PackIconBoxIconsKind.RegularQr);
            services.AddAppModule<FileSharingViewModel, FileSharingView>("File Sharing", PackIconBoxIconsKind.RegularShare);
            services.AddAppModule<ClipboardSyncViewModel, ClipboardSyncView>("Clipboard Sync", PackIconBoxIconsKind.RegularSync);

            services.AddTransient<RemoteControlViewModel>();

            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IRegistryService, RegistryService>();
            services.AddSingleton<IPowerControl, PowerControl>();
            services.AddSingleton<ISystemTime, SystemTime>();
            services.AddSingleton<IProcessService, ProcessManager>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IKeyboardHookManager, KeyboardHookManager>();
            services.AddSingleton<ITrayService, TrayService>();
            services.AddSingleton<IUiDispatcher, UiDispatcher>();
            services.AddSingleton<IEnvironmentHelper, EnvironmentHelper>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton(services => DialogCoordinator.Instance);
            services.AddSingleton<ISettings, Settings>();
            services.AddSingleton<IServerUriProvider>(services => services.GetRequiredService<ISettings>());
            services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
            services.AddSingleton<IThemeSetter, ThemeSetter>();
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IFileLockSearcher, FileLockSearcher>();
            services.AddScoped<ICapturePicker, CapturePicker>();
            services.AddScoped<IScreenGrabber, ScreenGrabber>();
            services.AddScoped<IScreenRecorder, ScreenRecorder>();
            services.AddScoped<IMetadataReader, MetadataReader>();
            services.AddScoped<IJobRunner, JobRunner>();
            services.AddScoped<IReportWriter, ReportWriter>();
            services.AddScoped<IPathTransformer, PathTransformer>();
            services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();
            services.AddScoped<IDtoHandler, DtoHandler>();
            services.AddTransient<IHubConnectionBuilder, HubConnectionBuilder>();

            services.AddSingleton<IUpdateChecker, UpdateChecker>();
            services.AddSingleton(services => (IBackgroundService)services.GetRequiredService<IUpdateChecker>());
            services.AddSingleton<IDesktopHubConnection, DesktopHubConnection>();
            services.AddSingleton(services => (IBackgroundService)services.GetRequiredService<IDesktopHubConnection>());

            services.AddHttpClient<IFileApi, FileApi>();
            services.AddHttpClient<IClipboardApi, ClipboardApi>();
            services.AddHttpClient<IStreamingApi, StreamingApi>();


            services.AddLogging(builder => builder.AddProvider(new FileLoggerProvider()));

            _instance = services.BuildServiceProvider();

            return _instance;
        }
        finally
        {
            _buildLock.Release();
        }
    }
}
