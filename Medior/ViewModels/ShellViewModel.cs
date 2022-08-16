using System;
using System.Collections.Generic;
using System.Linq;
using Medior.Models;
using MahApps.Metro.IconPacks;
using Medior.Views;
using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using Medior.Shared.Interfaces;
using System.Diagnostics;
using Medior.Shared.Services;

namespace Medior.ViewModels
{
    public class ShellViewModel : ObservableObjectEx
    {
        private readonly IDialogCoordinator _dialogs;
        private readonly IEncryptionService _encryption;
        private readonly IMessenger _messenger;
        private readonly ISettings _settings;
        private readonly IWindowService _windowService;
        public ShellViewModel(
            IMessenger messeger,
            ISettings settings,
            IWindowService windowService,
            IDialogCoordinator dialogs,
            IEncryptionService encryption,
            IEnumerable<AppModule> appModules)
        {
            _settings = settings;
            _messenger = messeger;
            _windowService = windowService;
            _dialogs = dialogs;
            _encryption = encryption;
            AppModules.AddRange(appModules);
            FilteredAppModules.AddRange(appModules);

            _messenger.Register<GenericMessage<HotKeyHookKind>>(this, HandleHotKeyInvocation);
            _messenger.Register<LoaderUpdateMessage>(this, HandleLoaderUpdate);
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


        public async Task LoadPrivateKey()
        {
            if (!_settings.IsAccountEnabled)
            {
                return;
            }

            while (!_settings.PrivateKeyBytes.Any())
            {
                var input = await _dialogs.ShowLoginAsync(
                    this,
                    "Enter Your PIN/Password",
                    "You must enter your password to decrypt your private key.\n",
                    new LoginDialogSettings()
                    {
                        ShouldHideUsername = true,
                        PasswordWatermark = "Enter your PIN"
                    });

                if (string.IsNullOrWhiteSpace(input.Password))
                {
                    return;
                }

                var result = _encryption.ImportPrivateKey(input.Password, _settings.EncryptedPrivateKeyBytes);

                if (result.IsSuccess)
                {
                    _settings.PrivateKeyBytes = result.Value!.PrivateKey;
                    _settings.PublicKeyBytes = result.Value!.PublicKey;
                }

                if (!_settings.PrivateKeyBytes.Any())
                {
                    var tryAgainResult = await _dialogs.ShowMessageAsync(
                        this,
                        "Try Again?",
                        "PIN/password is incorrect.  Private key decryption failed.  Do you want to try again?\n",
                        MessageDialogStyle.AffirmativeAndNegative,
                        new MetroDialogSettings()
                        {
                            AffirmativeButtonText = "Yes",
                            NegativeButtonText = "No",
                            DefaultButtonFocus = MessageDialogResult.Affirmative
                        });

                    if (tryAgainResult != MessageDialogResult.Affirmative)
                    {
                        await _dialogs.ShowMessageAsync(this, "Shutting Down",
                            "Medior cannot start without decrypting the private key.  The application will " +
                            "now shut down.  You can delete your settings file to start over with a fresh state.\n");
                        WpfApp.Current.Shutdown();
                    }
                }
            }
        }

        private async void HandleHotKeyInvocation(object recipient, GenericMessage<HotKeyHookKind> message)
        {
            switch (message.Value)
            {
                case HotKeyHookKind.PrintScreen:
                    {
                        SelectedModule = AppModules.FirstOrDefault(x => x.ControlType == typeof(ScreenCaptureView));
                        await Task.Delay(10);
                        _messenger.SendGenericMessage(ScreenCaptureRequestKind.Snip);
                    }
                    break;
                default:
                    break;
            }
        }

        private void HandleLoaderUpdate(object recipient, LoaderUpdateMessage message)
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
    }
}
