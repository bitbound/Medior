using Medior.Reactive;
using Medior.Services;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Medior.Models;
using MahApps.Metro.IconPacks;
using Medior.Views;

namespace Medior.ViewModels
{
    internal interface IShellViewModel
    {
        ObservableCollectionEx<AppModule> AppModules { get; }
        bool IsLoaderVisible { get; set; }
        string LoaderText { get; set; }
        List<AppModule> OptionsModules { get; }
    }

    internal class ShellViewModel : ObservableObjectEx, IShellViewModel
    {
        private readonly ILoaderService _loader;

        public ShellViewModel(ILoaderService loader, IEnumerable<AppModule> appModules)
        {
            _loader = loader;
            AppModules.AddRange(appModules);

            _loader.LoaderShown += Loader_LoaderShown;
            _loader.LoaderHidden += Loader_LoaderHidden;
        }

        public ObservableCollectionEx<AppModule> AppModules { get; } = new();

        public bool IsLoaderVisible
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string LoaderText
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public List<AppModule> OptionsModules { get; } = new()
        {
             new AppModule(
                "About",
                new PackIconOcticons() { Kind = PackIconOcticonsKind.Question },
                typeof(AboutView)),

            new AppModule(
                "Settings",
                new PackIconOcticons() { Kind = PackIconOcticonsKind.Settings },
                typeof(SettingsView))
        };
        private void Loader_LoaderHidden(object? sender, EventArgs e)
        {
            IsLoaderVisible = false;
            LoaderText = string.Empty;
        }

        private void Loader_LoaderShown(object? sender, string message)
        {
            LoaderText = message;
            IsLoaderVisible = true;
        }
    }
}
