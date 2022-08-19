using MahApps.Metro.Controls.Dialogs;
using Medior.Interfaces;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using Medior.Shared.Services.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IUpdateChecker
    {
        bool IsNewVersionAvailable { get; }

        Task CheckForUpdates(bool promptToDownload);
    }
    public class UpdateChecker : IUpdateChecker, IBackgroundService
    {
        private readonly IFileApi _fileApi;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
        private readonly IDialogService _dialogs;
        private readonly IEnvironmentHelper _environment;
        private readonly ILogger<UpdateChecker> _logger;
        private readonly IMessenger _messenger;
        private readonly IProcessService _processes;
        private readonly IWindowService _windowService;

        public UpdateChecker(
            IFileApi fileApi, 
            IMessenger messenger,
            IDialogService dialogs,
            IEnvironmentHelper environmentHelper,
            IProcessService processes,
            IWindowService windowService,
            ILogger<UpdateChecker> logger)
        {
            _fileApi = fileApi;
            _messenger = messenger;
            _dialogs = dialogs;
            _processes = processes;
            _windowService = windowService;
            _environment = environmentHelper;
            _logger = logger;
        }

        public bool IsNewVersionAvailable { get; private set; }

        public async Task CheckForUpdates(bool promptToDownload)
        {
            try
            {
                using var scope = _logger.BeginScope(nameof(CheckForUpdates));

                var result = await _fileApi.GetDesktopVersion();

                if (!result.IsSuccess)
                {
                    throw result.Exception!;
                }

                var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var remoteVersion = result.Value;

                if (localVersion == remoteVersion)
                {
                    _logger.LogInformation("Version is current.");
                    return;
                }

                _logger.LogInformation("New version available: {version}", remoteVersion);
                IsNewVersionAvailable = true;

                if (promptToDownload)
                {
                    await PromptToUpdate();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking for updates.");
            }
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            if (_environment.IsDebug)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForUpdates(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking for updates.");
                }

                await Task.Delay(_checkInterval, cancellationToken);
            }
        }

        private async Task PromptToUpdate()
        {
            _windowService.ShowMainWindow();
            var result = await _dialogs.ShowMessageAsync(
                    "Update Available",
                    "A new version is available.  Would you like to update now?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new() { DefaultButtonFocus = MessageDialogResult.Affirmative });

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }


            var downloadResult = await _fileApi.DownloadDesktopSetup();

            if (!downloadResult.IsSuccess)
            {
                _messenger.SendToast("Failed to download update", ToastType.Error);
                return;
            }

            var psi = new ProcessStartInfo()
            {
                FileName = downloadResult.Value!,
                UseShellExecute = true
            };
            _processes.Start(psi);
        }
    }
}
