using Medior.Interfaces;
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
        Task<string> GetClipboardReceiveUrl();
        Task<string> GetClipboardSendUrl();
    }

    internal class DesktopHubConnection : HubConnectionBase, IDesktopHubConnection, IBackgroundService
    {
        private readonly IServerUriProvider _serverUri;
        private readonly ILogger<DesktopHubConnection> _logger;

        public DesktopHubConnection(
            IHubConnectionBuilder builder,
            IServerUriProvider serverUri,
            IDtoHandler dtoHandler,
            ILogger<DesktopHubConnection> logger) 
            : base(builder, dtoHandler, logger)
        {
            _serverUri = serverUri;
            _logger = logger;
        }

        public Task<string> GetClipboardReceiveUrl()
        {
            throw new NotImplementedException();
        }

        public Task<string> GetClipboardSendUrl()
        {
            throw new NotImplementedException();
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await Connect($"{_serverUri.ServerUri}/hubs/desktop", ConfigureConnection, cancellationToken);
        }

        private void ConfigureConnection(HubConnection connection)
        {
            
        }
    }
}
