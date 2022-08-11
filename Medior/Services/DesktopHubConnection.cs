using Medior.Interfaces;
using Medior.Shared;
using Medior.Shared.Dtos;
using Medior.Shared.Dtos.Enums;
using Medior.Shared.Interfaces;
using Medior.Shared.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IDesktopHubConnection
    {
        Task<Result<string>> GetClipboardReceiveUrl();
    }

    internal class DesktopHubConnection : HubConnectionBase, IDesktopHubConnection, IBackgroundService
    {
        private readonly IServerUriProvider _serverUri;
        private readonly IMessenger _messenger;
        private readonly ILogger<DesktopHubConnection> _logger;

        public DesktopHubConnection(
            IHubConnectionBuilder builder,
            IServerUriProvider serverUri,
            IDtoHandler dtoHandler,
            IMessenger messenger,
            ILogger<DesktopHubConnection> logger) 
            : base(builder, dtoHandler, logger)
        {
            _serverUri = serverUri;
            _messenger = messenger;
            _logger = logger;
        }

        public async Task<Result<string>> GetClipboardReceiveUrl()
        {
            return await TryInvoke(async (hubConnection) =>
            {
                return await hubConnection.InvokeAsync<Result<string>>(nameof(GetClipboardReceiveUrl));
            });
        }


        public async Task Start(CancellationToken cancellationToken)
        {
            await Connect($"{_serverUri.ServerUri}/hubs/desktop", ConfigureConnection, cancellationToken);
        }

        private void ConfigureConnection(HubConnection connection)
        {
            
        }


        private async Task<Result<T>> TryInvoke<T>(Func<HubConnection, Task<Result<T>>> hubInvocation)
        {
            var getConnectionResult = await GetConnection();
            if (!getConnectionResult.IsSuccess)
            {
                _messenger.SendToast(getConnectionResult.Error!, ToastType.Warning);
                return Result.Fail<T>(getConnectionResult.Exception!);
            }

            return await hubInvocation(getConnectionResult.Value!);
        }
    }
}
