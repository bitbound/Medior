using Microsoft.Extensions.DependencyInjection;
using System;

namespace Medior.ViewModels;

internal static class ViewModelLocator
{
    public static AboutViewModel AboutViewModel => StaticServiceProvider.Instance.GetRequiredService<AboutViewModel>();
    public static ClipboardSyncViewModel ClipboardSyncViewModel => StaticServiceProvider.Instance.GetRequiredService<ClipboardSyncViewModel>();
    public static FileSharingViewModel FileSharingViewModel => StaticServiceProvider.Instance.GetRequiredService<FileSharingViewModel>();
    public static KeepAwakeViewModel KeepAwakeViewModel => StaticServiceProvider.Instance.GetRequiredService<KeepAwakeViewModel>();
    public static PhotoSorterViewModel PhotoSorterViewModel => StaticServiceProvider.Instance.GetRequiredService<PhotoSorterViewModel>();
    public static QrCodeGeneratorViewModel QrCodeGeneratorViewModel => StaticServiceProvider.Instance.GetRequiredService<QrCodeGeneratorViewModel>();
    public static RemoteControlViewModel RemoteControlViewModel => StaticServiceProvider.Instance.GetRequiredService<RemoteControlViewModel>();
    public static ScreenCaptureViewModel ScreenCaptureViewModel => StaticServiceProvider.Instance.GetRequiredService<ScreenCaptureViewModel>();
    public static SettingsViewModel SettingsViewModel => StaticServiceProvider.Instance.GetRequiredService<SettingsViewModel>();
    public static ShellViewModel ShellWindowViewModel => StaticServiceProvider.Instance.GetRequiredService<ShellViewModel>();
    public static ToastsViewModel ToastsViewModel => StaticServiceProvider.Instance.GetRequiredService<ToastsViewModel>();
}
