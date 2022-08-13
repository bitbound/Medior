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
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface ISettings: IServerUriProvider
    {
        UploadedFile[] FileUploads { get; set; }
        bool HandlePrintScreen { get; set; }
        bool IsNavPaneOpen { get; set; }
        SortJob[] SortJobs { get; set; }
        bool StartAtLogon { get; set; }
        AppTheme Theme { get; set; }
        ClipboardSaveDto[] ClipboardSaves { get; set; }

        Task Save();
    }
    public class Settings : ISettings
    {
        private readonly IEnvironmentHelper _environmentHelper;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly string _filePath = AppConstants.SettingsFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IKeyboardHookManager _keyboardHookManager;
        private readonly ILogger<Settings> _logger;
        private readonly IRegistryService _registryService;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };
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

        public bool IsNavPaneOpen
        {
            get => Get<bool>();
            set => Set(value);
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
        public ClipboardSaveDto[] ClipboardSaves
        {
            get => Get<ClipboardSaveDto[]>() ?? Array.Empty<ClipboardSaveDto>();
            set => Set(value);
        }

        public async Task Save()
        {
            if (!await _fileLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                _fileSystem.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                var serializedModel = JsonSerializer.Serialize(_settings, _serializerOptions);
                await _fileSystem.WriteAllTextAsync(_filePath, serializedModel);
                _fileSystem.Encrypt(_filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings.");
            }
            finally
            {
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

                _fileSystem.CreateDirectory(Path.GetDirectoryName(_filePath)!);

                if (!_fileSystem.FileExists(_filePath))
                {
                    return Result.Ok();
                }

                var serializedModel = _fileSystem.ReadAllText(_filePath);
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
