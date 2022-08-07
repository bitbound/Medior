using MahApps.Metro.Controls.Dialogs;
using Medior.Interfaces;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
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
    }
    public class UpdateChecker : IUpdateChecker, IBackgroundService
    {
        private readonly IApiService _api;
        private readonly IMessenger _messenger;
        private readonly IDialogService _dialogs;
        private readonly IServerUriProvider _serverUri;
        private readonly IProcessService _processes;
        private readonly IEnvironmentHelper _environment;
        private readonly ILogger<UpdateChecker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6);

        public UpdateChecker(
            IApiService apiService, 
            IMessenger messenger,
            IServerUriProvider serverUriProvider,
            IDialogService dialogs,
            IEnvironmentHelper environmentHelper,
            IProcessService processes,
            ILogger<UpdateChecker> logger)
        {
            _api = apiService;
            _messenger = messenger;
            _dialogs = dialogs;
            _serverUri = serverUriProvider;
            _processes = processes;
            _environment = environmentHelper;
            _logger = logger;
        }

        public bool IsNewVersionAvailable { get; private set; }

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
                    var result = await _api.GetDesktopVersion();

                    if (!result.IsSuccess)
                    {
                        throw result.Exception!;
                    }

                    var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    var remoteVersion = result.Value;

                    if (localVersion != remoteVersion)
                    {
                        IsNewVersionAvailable = true;
                        await PromptToUpdate();
                    }
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
            var result = await _dialogs.ShowMessageAsync(
                    "Update Available",
                    "A new version is available.  Would you like to update now?",
                    MessageDialogStyle.AffirmativeAndNegative,
                    new() { DefaultButtonFocus = MessageDialogResult.Affirmative });

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }


            var downloadResult = await _api.DownloadDesktopSetup();

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
