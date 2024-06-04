using MahApps.Metro.Controls.Dialogs;
using Medior.Models;
using Medior.Shared;
using Medior.Shared.Entities;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Medior.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IDialogService _dialogs;
    private readonly IEncryptionService _encryption;
    private readonly IFileSystem _fileSystem;
    private readonly IDesktopHubConnection _hubConnection;
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly IMessenger _messenger;
    private readonly ISettings _settings;
    private readonly IThemeSetter _themeSetter;

    public SettingsViewModel(
        ISettings settings,
        IThemeSetter themeSetter,
        IMessenger messenger,
        ILogger<SettingsViewModel> logger,
        IEncryptionService encryption,
        IDialogService dialogs,
        IFileSystem fileSystem,
        IDesktopHubConnection hubConnection)
    {
        _settings = settings;
        _themeSetter = themeSetter;
        _logger = logger;
        _messenger = messenger;
        _encryption = encryption;
        _dialogs = dialogs;
        _fileSystem = fileSystem;
        _hubConnection = hubConnection;
    }

    public bool HandlePrintScreen
    {
        get => _settings.HandlePrintScreen;
        set => _settings.HandlePrintScreen = value;
    }

    public string ServerUri
    {
        get => _settings.ServerUri;
        set
        {
            _settings.SetServerUri(value);
            _ = CheckServerConnection();
        }
    }

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
    private async Task ChangeSettingsFilePath()
    {
        var sfd = new SaveFileDialog()
        {
            Filter = "JSON Files (*.json)|*.json",
            AddExtension = true,
            DefaultExt = ".json",
            OverwritePrompt = false
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

    private async Task CheckServerConnection()
    {
        var result = await _hubConnection.CheckConnection();
        if (!result.IsSuccess)
        {
            await _dialogs.ShowError("Connection failed.  Please check the server URL.");
            return;
        }
        _messenger.SendToast("Reconnected to server", ToastType.Success);
    }


    [RelayCommand]
    private void SetTheme(AppTheme theme)
    {
        Theme = theme;
    }
}
