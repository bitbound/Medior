using Microsoft.Extensions.DependencyInjection;
using System;

namespace Medior.ViewModels
{
    internal static class ViewModelLocator
    {
        public static IAboutViewModel AboutViewModel => StaticServiceProvider.Instance.GetRequiredService<IAboutViewModel>();
        public static IKeepAwakeViewModel KeepAwakeViewModel => StaticServiceProvider.Instance.GetRequiredService<IKeepAwakeViewModel>();
        public static IPhotoSorterViewModel PhotoSorterViewModel => StaticServiceProvider.Instance.GetRequiredService<IPhotoSorterViewModel>();
        public static IScreenCaptureViewModel ScreenCaptureViewModel => StaticServiceProvider.Instance.GetRequiredService<IScreenCaptureViewModel>();
        public static ISettingsViewModel SettingsViewModel => StaticServiceProvider.Instance.GetRequiredService<ISettingsViewModel>();
        public static IShellViewModel ShellWindowViewModel => StaticServiceProvider.Instance.GetRequiredService<IShellViewModel>();
        public static IToastsViewModel ToastsViewModel => StaticServiceProvider.Instance.GetRequiredService<IToastsViewModel>();
    }
}
