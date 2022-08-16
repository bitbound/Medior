using MahApps.Metro.Controls.Dialogs;
using Medior.Helpers;
using Medior.Models;
using Medior.Shared;
using Medior.Shared.Auth;
using Medior.Shared.Entities;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Medior.ViewModels
{
    [ObservableObject]
    public partial class SettingsViewModel
    {
        private readonly IAccountApi _accountApi;
        private readonly IDialogService _dialogs;
        private readonly IEncryptionService _encryption;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<SettingsViewModel> _logger;
        private readonly IMessenger _messenger;
        private readonly ISettings _settings;
        private readonly IThemeSetter _themeSetter;
        private readonly IWindowService _windowService;

        public SettingsViewModel(
            ISettings settings,
            IThemeSetter themeSetter,
            IMessenger messenger,
            ILogger<SettingsViewModel> logger,
            IEncryptionService encryption,
            IDialogService dialogs,
            IFileSystem fileSystem,
            IAccountApi accountApi,
            IWindowService windowService)
        {
            _settings = settings;
            _themeSetter = themeSetter;
            _windowService = windowService;
            _logger = logger;
            _messenger = messenger;
            _encryption = encryption;
            _dialogs = dialogs;
            _fileSystem = fileSystem;
            _accountApi = accountApi;
            SetThemeCommand = new RelayCommand<AppTheme>(parameter =>
            {
                Theme = parameter;
            });
        }

        public bool HandlePrintScreen
        {
            get => _settings.HandlePrintScreen;
            set => _settings.HandlePrintScreen = value;
        }

        public bool IsAccountEnabled => _settings.IsAccountEnabled;

        public ICommand SetThemeCommand { get; }

        public string SettingsFilePath
        {
            get => _settings.SettingsFilePath;
        }
        public bool StartAtLogon
        {
            get => _settings.StartAtLogon;
            set => _settings.StartAtLogon = value;
        }

        public AppTheme Theme
        {
            get => _settings.Theme;
            set
            {
                _settings.Theme = value;
                _themeSetter.SetTheme(value);
            }
        }

        [RelayCommand]
        public void ShowAccountHelp()
        {
            _windowService.ShowAccountHelp();
        }

        [RelayCommand]
        private async Task ChangeSettingsFilePath()
        {
            var sfd = new SaveFileDialog()
            {
                Filter = "JSON Files (*.json)|*.json",
                AddExtension = true,
                DefaultExt = ".json"
            };

            var result = sfd.ShowDialog();

            if (result != DialogResult.OK)
            {
                return;
            }

            var importExisting = false;

            if (_fileSystem.FileExists(sfd.FileName))
            {
                var dialogSettings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Import",
                    NegativeButtonText = "Overwrite",
                    FirstAuxiliaryButtonText = "Abort",
                    DefaultButtonFocus = MessageDialogResult.FirstAuxiliary
                };
                var actionResult = await _dialogs.ShowMessageAsync(
                    "Choose Action",
                    "A file already exists at the chosen path.  You can import it and overwrite your current " +
                        "settings, overwrite the file with your current settings, or abort.",
                    MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary,
                    dialogSettings);


                switch (actionResult)
                {
                    case MessageDialogResult.Affirmative:
                        importExisting = true;
                        break;
                    case MessageDialogResult.Negative:
                        importExisting = false;
                        break;
                    case MessageDialogResult.Canceled:
                    case MessageDialogResult.FirstAuxiliary:
                    case MessageDialogResult.SecondAuxiliary:
                    default:
                        return;
                }
            }

            Result changeResult;
            if (importExisting)
            {
                changeResult = await _settings.ChangeSettingsFilePath(sfd.FileName, true);
            }
            else
            {
                changeResult = await _settings.ChangeSettingsFilePath(sfd.FileName, false);
            }

            if (!changeResult.IsSuccess)
            {
                await _dialogs.ShowError(changeResult.Exception!);
                return;
            }

            _messenger.SendToast("Settings path changed", ToastType.Success);
        }

        [RelayCommand]
        private async Task CreateAccount()
        {

            try
            {
                if (IsAccountEnabled)
                {
                    _logger.LogWarning("Unexpected state when creating account: Account alredy exists.");
                    _messenger.SendToast("Account already exists", ToastType.Warning);
                    return;
                }

                var response1 = await _dialogs.ShowLoginAsync(
                    "Create an Account",
                    "Choose a PIN or password to use for decrypting your local private key.\n\n" +
                        "Warning: Your password is not stored anywhere and cannot be recovered. There is no password reset.  " +
                        "Make sure you back up your settings file, which contains your encrypted private key, in a safe place.\n\n",
                    new LoginDialogSettings()
                    {
                        PasswordWatermark = "PIN or password...",
                        AffirmativeButtonText = "Submit"
                    });


                if (string.IsNullOrWhiteSpace(response1.Username))
                {
                    await _dialogs.ShowMessageAsync("Username Required", "A username is required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(response1.Password))
                {
                    await _dialogs.ShowMessageAsync("Password Required", "A password is required.");
                    return;
                }

                if (!Regex.IsMatch(response1.Username, "^((\\w|-)* {0,1}([a-zA-Z0-9])+)+$"))
                {
                    await _dialogs.ShowMessageAsync("Invalid Pattern",
                        "Username can only contain alphanumeric characters, underscores, and hyphens. " +
                        "It must end with an alphanumeric character.  It cannot have two consecutive spaces.");
                    return;
                }

                var response2 = await _dialogs.ShowLoginAsync(
                    "Confirm Your PIN/Password",
                    "Enter your password a second time to confirm.\n\n",
                    new LoginDialogSettings()
                    {
                        ShouldHideUsername = true,
                        PasswordWatermark = "Enter again to confirm...",
                        AffirmativeButtonText = "Submit"
                    });

                if (response1.Password != response2.Password)
                {
                    _messenger.SendToast("Passwords did not match.", ToastType.Error);
                    return;
                }

                var keys = _encryption.GenerateKeys(response1.Password);

                var account = new UserAccount()
                {
                    PublicKey = keys.PublicKeyBase64,
                    Username = response1.Username.Trim()
                };

                HttpHelper.UpdateClientAuthorization(_accountApi.Client, account, _encryption);

                var accountResult = await _accountApi.CreateAccount(account);

                if (!accountResult.IsSuccess)
                {
                    await _dialogs.ShowError(accountResult.Exception!);
                    return;
                }

                _settings.Username = account.Username;
                _settings.PublicKeyBytes = keys.PublicKey;
                _settings.PrivateKeyBytes = keys.PrivateKey;
                _settings.EncryptedPrivateKeyBytes = keys.EncryptedPrivateKey;

                _messenger.SendToast("Account created", ToastType.Success);
            }
            catch (Exception ex)
            {
                _settings.PublicKeyBytes = Array.Empty<byte>();
                _settings.PrivateKeyBytes = Array.Empty<byte>();
                _settings.EncryptedPrivateKeyBytes = Array.Empty<byte>();
                _settings.Username = string.Empty;
                _logger.LogError(ex, "Error while creating new account.");
                await _dialogs.ShowError(ex);
            }
            finally
            {
                OnPropertyChanged(nameof(IsAccountEnabled));
            }


        }

        [RelayCommand]
        private async Task DeleteAccount()
        {
            try
            {
                var result = await _dialogs.ShowMessageAsync(
                  "Confirm Deletion",
                  "Are you sure you want to delete your account (public/private key)?\n\n" +
                      "You will lose access to your encrypted files and your contacts.  Online " +
                      "features that depend on an account, such as file encryption, will stop working.",
                  MessageDialogStyle.AffirmativeAndNegative);

                if (result != MessageDialogResult.Affirmative)
                {
                    return;
                }

                var accountResult = await _accountApi.DeleteAccount();

                if (!accountResult.IsSuccess)
                {
                    await _dialogs.ShowError(accountResult.Exception!);
                    return;
                }

                _settings.PublicKeyBytes = Array.Empty<byte>();
                _settings.PrivateKeyBytes = Array.Empty<byte>();
                _settings.EncryptedPrivateKeyBytes = Array.Empty<byte>();
                _settings.Username = string.Empty;
                _encryption.Reset();

                _messenger.SendToast("Account deleted", ToastType.Success);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting account.");
                await _dialogs.ShowError(ex);
            }
        }

        [RelayCommand]
        private async Task RegenerateKeys()
        {
            try
            {
                var result = await _dialogs.ShowMessageAsync(
                    "Confirm Regenerate",
                    "Are you sure you want to regenerate your public and private keys?\n\n" +
                        "You will need to pair with your contacts again, and encrypted files will " +
                        "become inaccessible.",
                    MessageDialogStyle.AffirmativeAndNegative);

                if (result != MessageDialogResult.Affirmative)
                {
                    return;
                }

                var response1 = await _dialogs.ShowLoginAsync(
                    "Create a PIN/Password",
                    "Enter a PIN/password to use for decrypting your new private key.\n\n",
                    new LoginDialogSettings()
                    {
                        ShouldHideUsername = true,
                        PasswordWatermark = "Enter again to confirm...",
                        AffirmativeButtonText = "Submit"
                    });

                var response2 = await _dialogs.ShowLoginAsync(
                    "Confirm Your PIN/Password",
                    "Enter your password a second time to confirm.\n\n",
                    new LoginDialogSettings()
                    {
                        ShouldHideUsername = true,
                        PasswordWatermark = "Enter again to confirm...",
                        AffirmativeButtonText = "Submit"
                    });

                _encryption.SaveState();


                var keys = _encryption.GenerateKeys(response1.Password);

                var account = new UserAccount()
                {
                    PublicKey = keys.PublicKeyBase64,
                    Username = _settings.Username
                };

                HttpHelper.UpdateClientAuthorization(_accountApi.Client, account, _encryption);

                var accountResult = await _accountApi.UpdatePublicKey(account);

                if (!accountResult.IsSuccess)
                {
                    await _dialogs.ShowError(accountResult.Exception!);
                    return;
                }

                _settings.PublicKeyBytes = keys.PublicKey;
                _settings.Username = account.Username;
                _settings.PrivateKeyBytes = keys.PrivateKey;
                _settings.EncryptedPrivateKeyBytes = keys.EncryptedPrivateKey;

                _messenger.SendToast("Keys regenerated", ToastType.Success);
            }
            catch (Exception ex)
            {
                var restoredKeys = _encryption.RestoreState();
                _settings.PublicKeyBytes = restoredKeys.PublicKey;
                _settings.PrivateKeyBytes = restoredKeys.PrivateKey;
                _settings.EncryptedPrivateKeyBytes = restoredKeys.EncryptedPrivateKey;
                _settings.Username = string.Empty;

                var account = new UserAccount()
                {
                    PublicKey = _settings.PublicKey,
                    Username = _settings.Username
                };

                HttpHelper.UpdateClientAuthorization(_accountApi.Client, account, _encryption);

                _logger.LogError(ex, "Error while regenerating keys.");
                await _dialogs.ShowError(ex);
            }
        }
    }
}
