using CommunityToolkit.Diagnostics;
using Medior.Models;
using Medior.Models.PhotoSorter;
using Medior.Shared;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface ISettings : IServerUriProvider
    {
        ClipboardSaveDto[] ClipboardSaves { get; set; }
        byte[] EncryptedPrivateKeyBytes { get; set; }
        UploadedFile[] FileUploads { get; set; }
        bool HandlePrintScreen { get; set; }
        bool IsAccountEnabled { get; }
        bool IsFileEncryptionEnabled { get; set; }
        bool IsNavPaneOpen { get; set; }
        public string PrivateKey { get; set; }
        byte[] PrivateKeyBytes { get; set; }
        public string PublicKey { get; set; }
        byte[] PublicKeyBytes { get; set; }
        string SettingsFilePath { get; }
        SortJob[] SortJobs { get; set; }
        bool StartAtLogon { get; set; }
        AppTheme Theme { get; set; }
        string Username { get; set; }
        Task<Result> ChangeSettingsFilePath(string filePath, bool importExistingFile);

        Task Save();
    }
    public class Settings : ISettings
    {
        private readonly IEnvironmentHelper _environmentHelper;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly SemaphoreSlim _saveLock = new(2, 2);
        private readonly string _filePath = AppConstants.SettingsFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IKeyboardHookManager _keyboardHookManager;
        private readonly ILogger<Settings> _logger;
        private readonly IRegistryService _registryService;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };
        private string _privateKey = string.Empty;
        private SettingsModel _settings = new();

        public Settings(
            IFileSystem fileSystem,
            IRegistryService registryService,
            IEnvironmentHelper environmentHelper,
            IKeyboardHookManager keyboardHookManager,
            ILogger<Settings> logger)
        {
            _fileSystem = fileSystem;
            _registryService = registryService;
            _environmentHelper = environmentHelper;
            _keyboardHookManager = keyboardHookManager;
            _logger = logger;
            Load();
        }

        public ClipboardSaveDto[] ClipboardSaves
        {
            get => Get<ClipboardSaveDto[]>() ?? Array.Empty<ClipboardSaveDto>();
            set => Set(value);
        }

        public string EncryptedPrivateKey
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public byte[] EncryptedPrivateKeyBytes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(EncryptedPrivateKey))
                {
                    return Array.Empty<byte>();
                }
                return Convert.FromBase64String(EncryptedPrivateKey);
            }
            set
            {
                EncryptedPrivateKey = Convert.ToBase64String(value);
            }
        }

        public UploadedFile[] FileUploads
        {
            get => Get<UploadedFile[]>() ?? Array.Empty<UploadedFile>();
            set => Set(value);
        }

        public bool HandlePrintScreen
        {
            get => Get<bool>();
            set
            {
                Set(value);
                if (value)
                {
                    _keyboardHookManager.SetPrintScreenHook();
                }
                else
                {
                    _keyboardHookManager.UnsetPrintScreenHook();
                }
            }
        }

        public bool IsAccountEnabled =>
            !string.IsNullOrWhiteSpace(PublicKey) &&
            EncryptedPrivateKeyBytes.Any();

        public bool IsFileEncryptionEnabled
        {
            get => Get<bool>();
            set => Set(value);
        }

        public bool IsNavPaneOpen
        {
            get => Get<bool>();
            set => Set(value);
        }

        public string PrivateKey
        {
            get => _privateKey;
            set => _privateKey = value;
        }

        public byte[] PrivateKeyBytes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_privateKey))
                {
                    return Array.Empty<byte>();
                }
                return Convert.FromBase64String(_privateKey);
            }
            set
            {
                _privateKey = Convert.ToBase64String(value);
            }
        }

        public string PublicKey
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }

        public byte[] PublicKeyBytes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(PublicKey))
                {
                    return Array.Empty<byte>();
                }
                return Convert.FromBase64String(PublicKey);
            }
            set
            {
                PublicKey = Convert.ToBase64String(value);
            }
        }
        public string ServerUri
        {
            get
            {
                if (_environmentHelper.IsDebug)
                {
                    return "https://localhost:7162";
                }

                var uri = Get<string>()?.TrimEnd('/');

                if (Uri.TryCreate(uri, UriKind.Absolute, out _))
                {
                    return uri;
                }

                return "https://medior.app";
            }
            set
            {
                if (_environmentHelper.IsDebug)
                {
                    return;
                }
                Set(value);
            }
        }

        public string SettingsFilePath
        {
            get
            {
                if (_environmentHelper.IsDebug)
                {
                    return _filePath;
                }
                return _registryService.GetSettingsFilePath() ?? _filePath;
            }
            private set => _registryService.SetSettingsFilePath(value);
        }

        public SortJob[] SortJobs
        {
            get => Get<SortJob[]>() ?? Array.Empty<SortJob>();
            set => Set(value);
        }

        public bool StartAtLogon
        {
            get => Get<bool>() && _registryService.GetStartAtLogon();
            set
            {
                Set(value);
                _registryService.SetStartAtLogon(value);
            }
        }

        public AppTheme Theme
        {
            get => Get<AppTheme>();
            set => Set(value);
        }

        public string Username
        {
            get => Get<string>() ?? string.Empty;
            set => Set(value);
        }
        public async Task<Result> ChangeSettingsFilePath(string filePath, bool importExistingFile)
        {
            var originalPath = SettingsFilePath;

            try
            {
                SettingsFilePath = filePath;

                if (_fileSystem.FileExists(filePath) && importExistingFile)
                {
                    var result = Load();
                    if (!result.IsSuccess)
                    {
                        throw result.Exception!;
                    }
                }
                else
                {
                    await Save();
                    //_fileSystem.DeleteFile(originalPath);
                }
                return Result.Ok();
            }
            catch (Exception ex)
            {
                SettingsFilePath = originalPath;
                _ = Load();
                _logger.LogError(ex, "Error while changing settings file path.");
                return Result.Fail(ex);
            }
        }

        public async Task Save()
        {
            if (!await _saveLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                await _fileLock.WaitAsync();
                _fileSystem.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);
                var serializedModel = JsonSerializer.Serialize(_settings, _serializerOptions);
                await _fileSystem.WriteAllTextAsync(SettingsFilePath, serializedModel);
                //_fileSystem.Encrypt(SettingsFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings.");
            }
            finally
            {
                _saveLock.Release();
                _fileLock.Release();
            }
        }



        private T? Get<T>([CallerMemberName] string propertyName = "", T? defaultValue = default)
        {
            var prop = _settings.GetType().GetProperty(propertyName);
            Guard.IsNotNull(prop, nameof(propertyName));

            var propValue = prop.GetValue(_settings);
            if (propValue is T typedValue)
            {
                return typedValue;
            }

            return defaultValue;
        }

        private Result Load()
        {
            try
            {
                _fileLock.Wait();

                _fileSystem.CreateDirectory(Path.GetDirectoryName(SettingsFilePath)!);

                if (!_fileSystem.FileExists(SettingsFilePath))
                {
                    return Result.Ok();
                }

                var serializedModel = _fileSystem.ReadAllText(SettingsFilePath);
                _settings = JsonSerializer.Deserialize<SettingsModel>(serializedModel) ?? new();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings.");
                return Result.Fail(ex);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private void Set<T>(T newValue, [CallerMemberName] string propertyName = "")
        {
            var prop = _settings.GetType().GetProperty(propertyName);
            prop?.SetValue(_settings, newValue);
            _ = Save();
        }
    }
}
