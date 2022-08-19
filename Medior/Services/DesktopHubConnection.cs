using Medior.Interfaces;
using Medior.Shared;
using Medior.Shared.Auth;
using Medior.Shared.Dtos;
using Medior.Shared.Interfaces;
using Medior.Shared.SignalR;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
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
        Task<Result<string>> GetClipboardReceiptToken();
        Task<Result> CheckConnection();
    }

    internal class DesktopHubConnection : HubConnectionBase, IDesktopHubClient, IDesktopHubConnection, IBackgroundService
    {
        private readonly IHttpConfigurer _httpConfig;
        private readonly ILogger<DesktopHubConnection> _logger;
        private readonly IMessenger _messenger;
        private readonly IServerUriProvider _serverUri;
        public DesktopHubConnection(
            IServiceScopeFactory scopeFactory,
            IServerUriProvider serverUri,
            IDtoHandler dtoHandler,
            IMessenger messenger,
            IHttpConfigurer httpConfigurer,
            ILogger<DesktopHubConnection> logger) 
            : base(scopeFactory, dtoHandler, logger)
        {
            _serverUri = serverUri;
            _messenger = messenger;
            _httpConfig = httpConfigurer;
            _logger = logger;


            _messenger.RegisterGeneric(this, HandleConnectionDetailsChanged);
        }

        public async Task<Result> CheckConnection()
        {
            var result = await GetConnection();
            if (!result.IsSuccess)
            {
                return Result.Fail("Connection is faulted.");
            }
            return Result.Ok();
        }

        public async Task<Result<string>> GetClipboardReceiptToken()
        {
            return await TryInvoke(async (hubConnection) =>
            {
                var receiptToken = await hubConnection.InvokeAsync<string>(nameof(GetClipboardReceiptToken));
                return Result.Ok(receiptToken);
            });
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await Connect(
                $"{_serverUri.ServerUri}/hubs/desktop",
                ConfigureConnection,
                ConfigureHttpOptions,
                cancellationToken);
        }

        private void ConfigureConnection(HubConnection connection)
        {

        }

        private void ConfigureHttpOptions(HttpConnectionOptions options)
        {
            var signature = _httpConfig.GetDigitalSignature();
            options.Headers.Add("Authorization", $"{AuthSchemes.DigitalSignature} {signature}");
        }
        private async void HandleConnectionDetailsChanged(object recipient, GenericMessage<ParameterlessMessageKind> message)
        {
            if (message.Value is ParameterlessMessageKind.PrivateKeyChanged or ParameterlessMessageKind.ServerUriChanged)
            {
                await Reconnect(
                    $"{_serverUri.ServerUri}/hubs/desktop",
                    ConfigureConnection,
                    ConfigureHttpOptions);
            }
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
