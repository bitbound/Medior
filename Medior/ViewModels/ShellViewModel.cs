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
using Microsoft.Toolkit.Mvvm.Messaging;
using Medior.Models.Messages;

namespace Medior.ViewModels
{
    public interface IShellViewModel
    {
        ObservableCollectionEx<AppModule> AppModules { get; }
        ObservableCollectionEx<AppModule> FilteredAppModules { get; }
        bool IsLoaderVisible { get; set; }
        double LoaderProgress { get; }
        string LoaderText { get; set; }
        LoaderType LoaderType { get; set; }
        List<AppModule> OptionsModules { get; }
        string SearchText { get; set; }
        AppModule? SelectedModule { get; set; }
    }

    public class ShellViewModel : ObservableObjectEx, IShellViewModel
    {
        private readonly IMessenger _messenger;

        public ShellViewModel(
            IMessenger messeger,
            IEnumerable<AppModule> appModules)
        {
            _messenger = messeger;
            AppModules.AddRange(appModules);
            FilteredAppModules.AddRange(appModules);

            _messenger.Register<PrintScreenInvokedMessage>(this, HandlePrintScreenInvoked);
            _messenger.Register<LoaderUpdate>(this, HandleLoaderUpdate);
        }

        public ObservableCollectionEx<AppModule> AppModules { get; } = new();

        public ObservableCollectionEx<AppModule> FilteredAppModules { get; } = new();

        public bool IsLoaderVisible
        {
            get => Get<bool>();
            set => Set(value);
        }

        public double LoaderProgress
        {
            get => Get<double>();
            set => Set(value);
        }
        public string LoaderText
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public LoaderType LoaderType
        {
            get => Get<LoaderType>();
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

        public string SearchText
        {
            get => Get<string>() ?? string.Empty;
            set
            {
                Set(value);
                FilteredAppModules.Clear();
                var filteredModules = AppModules.Where(x => x.Label.Contains(value.Trim(), StringComparison.OrdinalIgnoreCase));
                FilteredAppModules.AddRange(filteredModules);
            }
        }

        public AppModule? SelectedModule
        {
            get => Get<AppModule>() ?? AppModules.FirstOrDefault();
            set => Set(value);
        }

        private void HandleLoaderUpdate(object recipient, LoaderUpdate message)
        {
            IsLoaderVisible = message.IsShown;
            LoaderText = message.Text;
            LoaderProgress = message.LoaderProgress;
            LoaderType = message.Type;
        }
        private void HandlePrintScreenInvoked(object recipient, PrintScreenInvokedMessage message)
        {
            SelectedModule = AppModules.FirstOrDefault(x => x.ControlType == typeof(ScreenshotView));
        }
    }
}
