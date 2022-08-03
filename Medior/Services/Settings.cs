using Medior.Models;
using Medior.Native;
using Medior.Reactive;
using Medior.Shared;
using Medior.Shared.Interfaces;
using Medior.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface ISettings: IServerUriProvider
    {
        bool HandlePrintScreen { get; set; }
        bool StartAtLogon { get; set; }
        AppTheme Theme { get; set; }
        Task Save();
    }
    public class Settings : ISettings
    {
        private readonly IEnvironmentHelper _environmentHelper;
        private readonly SemaphoreSlim _fileLock = new(1, 1);
        private readonly string _filePath = AppConstants.SettingsFilePath;
        private readonly IFileSystem _fileSystem;
        private readonly IRegistryService _registryService;
        private readonly ILogger<Settings> _logger;
        private SettingsModel _settings = new();

        public Settings(
            IFileSystem fileSystem, 
            IRegistryService registryService,
            IEnvironmentHelper environmentHelper,
            ILogger<Settings> logger)
        {
            _fileSystem = fileSystem;
            _registryService = registryService;
            _environmentHelper = environmentHelper;
            _logger = logger;
            Load();
        }

        public bool HandlePrintScreen
        {
            get => Get<bool>();
            set
            {
                Set(value);
                if (value)
                {
                    PrintScreenHotkey.Set();
                }
                else
                {
                    PrintScreenHotkey.Unset();
                }
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
                return Get<string>()?.TrimEnd('/') ?? "https://medior.app";
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
        public async Task Save()
        {
            if (!await _fileLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                _fileSystem.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                var serializedModel = JsonSerializer.Serialize(_settings);
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
            var propValue = prop?.GetValue(_settings);
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
