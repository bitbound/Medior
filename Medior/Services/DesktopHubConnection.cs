using Medior.Interfaces;
using Medior.Shared;
using Medior.Shared.Auth;
using Medior.Shared.Interfaces;
using Medior.Shared.Models;
using Medior.Shared.SignalR;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services;

public interface IDesktopHubConnection
{
    Task<Result<string>> GetClipboardReceiptToken();
    Task<Result> CheckConnection();
    Task<Result> SendStream(Guid streamId, IAsyncEnumerable<VideoChunk> stream, CancellationToken cancellationToken);
}

internal class DesktopHubConnection : HubConnectionBase, IDesktopHubClient, IDesktopHubConnection, IBackgroundService
{
    private readonly ILogger<DesktopHubConnection> _logger;
    private readonly IMessenger _messenger;
    private readonly IServerUriProvider _serverUri;
    public DesktopHubConnection(
        IServiceScopeFactory scopeFactory,
        IServerUriProvider serverUri,
        IDtoHandler dtoHandler,
        IMessenger messenger,
        ILogger<DesktopHubConnection> logger) 
        : base(scopeFactory, dtoHandler, logger)
    {
        _serverUri = serverUri;
        _messenger = messenger;
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

    public async Task<Result> SendStream(Guid streamId, IAsyncEnumerable<VideoChunk> stream, CancellationToken cancellationToken)
    {
        return await TryInvoke(async (connection) =>
        {
            try
            {
                await connection.InvokeAsync("SendStream", streamId, stream, cancellationToken);
            }
            catch (TaskCanceledException) 
            {
                _logger.LogInformation("Send stream cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending stream.");
            }
            return Result.Ok();
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
            _logger.LogError(getConnectionResult.Exception!, "Failed to get connection.");
            _messenger.SendToast(getConnectionResult.Reason!, ToastType.Warning);
            return Result.Fail<T>(getConnectionResult.Exception!);
        }

        var invokeResult = await hubInvocation(getConnectionResult.Value!);
        if (!invokeResult.IsSuccess)
        {
            _logger.LogError(invokeResult.Exception!, "Failed to invoke hub method.");
        }
        return invokeResult;
    }

    private async Task<Result> TryInvoke(Func<HubConnection, Task<Result>> hubInvocation)
    {
        var result = await GetConnection();
        if (!result.IsSuccess)
        {
            _logger.LogError(result.Exception!, "Failed to get connection.");
            _messenger.SendToast(result.Reason!, ToastType.Warning);
            if (result.HadException)
            {
                return Result.Fail(result.Exception);
            }
            return Result.Fail(result.Reason);
        }

        var invokeResult = await hubInvocation(result.Value!);
        if (!invokeResult.IsSuccess)
        {
            _logger.LogError(invokeResult.Exception!, "Failed to invoke hub method.");
        }
        return invokeResult;
    }
}
