using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    internal static class ViewModelLocator
    {
        public static IAboutViewModel AboutViewModel => StaticServiceProvider.Instance.GetRequiredService<IAboutViewModel>();
        public static IShellViewModel ShellWindowViewModel => StaticServiceProvider.Instance.GetRequiredService<IShellViewModel>();
        public static ISettingsViewModel SettingsViewModel => StaticServiceProvider.Instance.GetRequiredService<ISettingsViewModel>();
    }
}
