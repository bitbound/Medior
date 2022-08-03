using System;
using System.Collections.Generic;
using System.Linq;
using Medior.Models;
using MahApps.Metro.IconPacks;
using Medior.Views;
using Microsoft.Toolkit.Mvvm.Messaging;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public interface IShellViewModel
    {
        ObservableCollectionEx<AppModule> AppModules { get; }
        ObservableCollectionEx<AppModule> FilteredAppModules { get; }
        bool IsLoaderVisible { get; set; }
        bool IsNavPaneOpen { get; set; }
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
        private readonly IWindowService _windowService;
        private readonly ISettings _settings;
        public ShellViewModel(
            IMessenger messeger,
            ISettings settings,
            IWindowService windowService,
            IEnumerable<AppModule> appModules)
        {
            _settings = settings;
            _messenger = messeger;
            _windowService = windowService;
            AppModules.AddRange(appModules);
            FilteredAppModules.AddRange(appModules);

            _messenger.Register<PrintScreenInvokedMessage>(this, HandlePrintScreenInvoked);
            _messenger.Register<LoaderUpdate>(this, HandleLoaderUpdate);
            _messenger.Register<NavigateRequestMessage>(this, HandleNavigateRequest);
        }

        public ObservableCollectionEx<AppModule> AppModules { get; } = new();

        public ObservableCollectionEx<AppModule> FilteredAppModules { get; } = new();

        public bool IsLoaderVisible
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsNavPaneOpen
        {
            get => _settings.IsNavPaneOpen;
            set => _settings.IsNavPaneOpen = value;
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
                new PackIconBoxIcons() { Kind = PackIconBoxIconsKind.RegularQuestionMark },
                typeof(AboutView)),

            new AppModule(
                "Settings",
                new PackIconBoxIcons() { Kind = PackIconBoxIconsKind.RegularWrench },
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

        private void HandleNavigateRequest(object recipient, NavigateRequestMessage message)
        {
            SelectedModule = AppModules.FirstOrDefault(x => x.ControlType == message.ControlType);
            _windowService.ShowMainWindow();
        }
        private async void HandlePrintScreenInvoked(object recipient, PrintScreenInvokedMessage message)
        {
            SelectedModule = AppModules.FirstOrDefault(x => x.ControlType == typeof(ScreenCaptureView));
            await Task.Delay(10);
            _messenger.Send(new ScreenCaptureRequest(CaptureKind.Snip));
        }
    }
}
