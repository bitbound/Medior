﻿using Medior.ViewModels;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using MahApps.Metro.IconPacks;
using Medior.Views;
using Microsoft.Toolkit.Mvvm.Messaging;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Services.PhotoSorter;

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

                services.AddSingleton<IAppStartup, AppStartup>();
                services.AddSingleton<IShellViewModel, ShellViewModel>();
                services.AddSingleton<ISettingsViewModel, SettingsViewModel>();
                services.AddSingleton<IToastsViewModel, ToastsViewModel>();
                services.AddSingleton<IAboutViewModel, AboutViewModel>();

                services.AddAppModule<HomeView>("Home", PackIconBoxIconsKind.RegularHome);
                services.AddAppModule<IScreenCaptureViewModel, ScreenCaptureViewModel, ScreenCaptureView>("Screen Capture", PackIconBoxIconsKind.RegularCamera);
                services.AddAppModule<IPhotoSorterViewModel, PhotoSorterViewModel, PhotoSorterView>("Photo Sorter", PackIconBoxIconsKind.RegularPhotoAlbum);
                services.AddAppModule<IKeepAwakeViewModel, KeepAwakeViewModel, KeepAwakeView>("Keep Awake", PackIconBoxIconsKind.RegularCoffee);

                services.AddSingleton<IFileSystem, FileSystem>();
                services.AddSingleton<IRegistryService, RegistryService>();
                services.AddSingleton<IPowerControl, PowerControl>();
                services.AddSingleton<ISystemTime, SystemTime>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<IKeyboardHookManager, KeyboardHookManager>();
                services.AddSingleton<ITrayService, TrayService>();
                services.AddSingleton<IApiService, ApiService>();
                services.AddSingleton<IUiDispatcher, UiDispatcher>();
                services.AddSingleton<IEnvironmentHelper, EnvironmentHelper>();
                services.AddSingleton(services => DialogCoordinator.Instance);
                services.AddSingleton<ISettings, Settings>();
                services.AddSingleton<IServerUriProvider>(services => services.GetRequiredService<ISettings>());
                services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                services.AddSingleton<IThemeSetter, ThemeSetter>();
                services.AddSingleton<IWindowService, WindowService>();
                services.AddScoped<IMessageSigner, MessageSigner>();
                services.AddScoped<ICapturePicker, CapturePicker>();
                services.AddScoped<IScreenGrabber, ScreenGrabber>();
                services.AddScoped<IScreenRecorder, ScreenRecorder>();
                services.AddScoped<IMetadataReader, MetadataReader>();
                services.AddScoped<IJobRunner, JobRunner>();
                services.AddScoped<IReportWriter, ReportWriter>();
                services.AddScoped<IPathTransformer, PathTransformer>();
                
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
