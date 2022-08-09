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
    internal class DesktopHubConnection : HubConnectionBase, IBackgroundService, IDesktopHubClient
    {
        private readonly IServerUriProvider _serverUri;
        private readonly ILogger<DesktopHubConnection> _logger;

        public DesktopHubConnection(
            IHubConnectionBuilder builder,
            IServerUriProvider serverUri,
            ILogger<DesktopHubConnection> logger) 
            : base(builder, logger)
        {
            _serverUri = serverUri;
            _logger = logger;
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
