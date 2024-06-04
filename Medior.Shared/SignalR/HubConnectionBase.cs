using System.Collections.Concurrent;
using Medior.Shared.Dtos;
using Medior.Shared.Helpers;
using Medior.Shared.Interfaces;
using MessagePack;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medior.Shared.SignalR;

public interface IHubConnectionBase
{
    Task ReceiveDto(DtoWrapper dto);
}

public abstract class HubConnectionBase : IHubConnectionBase
{
    private static readonly ConcurrentDictionary<Guid, byte[]> _dtoChunks = new();
    private readonly ILogger<HubConnectionBase> _baseLogger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDtoHandler _dtoHandler;
    private CancellationToken _cancellationToken;
    private HubConnection? _connection;

    public HubConnectionBase(
        IServiceScopeFactory scopeFactory,
        IDtoHandler dtoHandler,
        ILogger<HubConnectionBase> logger)
    {
        _scopeFactory = scopeFactory;
        _dtoHandler = dtoHandler;
        _baseLogger = logger;
    }

    public async Task Connect(
        string hubUrl, 
        Action<HubConnection> connectionConfig, 
        Action<HttpConnectionOptions> optionsConfig,
        CancellationToken cancellationToken)
    {
        if (_connection is not null && 
            _connection.State != HubConnectionState.Disconnected)
        {
            return;
        }

        _cancellationToken = cancellationToken;

        using var scope = _scopeFactory.CreateScope();
        var builder = scope.ServiceProvider.GetRequiredService<IHubConnectionBuilder>();
        
        _connection = builder
            .WithUrl(hubUrl, options =>
            {
                optionsConfig(options);
            })
            .AddMessagePackProtocol()
            .WithAutomaticReconnect(new RetryPolicy())
            .Build();

        _connection.On<DtoWrapper>("ReceiveDto", ReceiveDto);
        _connection.Reconnecting += HubConnection_Reconnecting;
        _connection.Reconnected += HubConnection_Reconnected;

        connectionConfig.Invoke(_connection);

        await StartConnection();
    }

    public async Task ReceiveDto(DtoWrapper dto)
    {
        await _dtoHandler.ReceiveDto(dto);
    }

    public async Task Reconnect(string hubUrl,
        Action<HubConnection> connectionConfig,
        Action<HttpConnectionOptions> optionsConfig)
    {
        if (_connection is not null)
        {
            await _connection.StopAsync();
        }

        await Connect(hubUrl, connectionConfig, optionsConfig, _cancellationToken);
    }

    protected async Task<Result<HubConnection>> GetConnection()
    {
        var result = await WaitHelper.WaitForAsync(() => _connection?.State == HubConnectionState.Connected, TimeSpan.FromSeconds(3));
        if (!result)
        {
            _baseLogger.LogError("Unable to establish a connection with the server.");
            return Result.Fail<HubConnection>("Unable to establish a connection with the server.");
        }
        return Result.Ok(_connection!);
    }

    protected async Task SendDtoToPeer<T>(T dto, DtoType dtoType, Guid requestId, HubType peerHubType, string requesterConnectionId)
    {
        foreach (var wrapper in DtoChunker.ChunkDto(dto, dtoType, requestId))
        {
            await _connection!.InvokeAsync("SendDtoToPeer", wrapper, peerHubType, requesterConnectionId);
        }
    }

    protected async Task<Result<T>> WaitForResponse<T>(
        HubConnection connection,
        string methodName,
        Guid requestId,
        Action sendAction,
        int timeoutMs = 5_000)
    {
        try
        {
            T? returnValue = default;
            var signal = new SemaphoreSlim(0, 1);

            using var token = connection.On<DtoWrapper>(methodName, wrapper =>
            {
                try
                {
                    if (wrapper.RequestId == requestId)
                    {
                        _dtoChunks.AddOrUpdate(requestId, wrapper.DtoChunk, (k, v) =>
                        {
                            return v.Concat(wrapper.DtoChunk).ToArray();
                        });

                        if (wrapper.IsLastChunk && _dtoChunks.TryRemove(requestId, out var concatChunks))
                        {
                            returnValue = MessagePackSerializer.Deserialize<T>(concatChunks);
                            signal.Release();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _baseLogger.LogError(ex, "Error while handling DTO wrapper.");
                }

            });

            sendAction.Invoke();

            var waitResult = await signal.WaitAsync(timeoutMs);

            if (!waitResult)
            {
                return Result.Fail<T>("Timed out while waiting for response.");
            }

            if (returnValue is null)
            {
                return Result.Fail<T>("Response was empty.");
            }

            return Result.Ok(returnValue);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>(ex);
        }
    }

    private Task HubConnection_Reconnected(string? arg)
    {
        _baseLogger.LogInformation("Reconnected to desktop hub.");
        return Task.CompletedTask;
    }

    private Task HubConnection_Reconnecting(Exception? arg)
    {
        _baseLogger.LogWarning(arg, "Reconnecting to desktop hub.");
        return Task.CompletedTask;
    }

    private async Task StartConnection()
    {
        if (_connection is null)
        {
            _baseLogger.LogWarning("Connection shouldn't be null here.");
            return;
        }

        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                _baseLogger.LogInformation("Connecting to server.");

                await _connection.StartAsync(_cancellationToken);

                _baseLogger.LogInformation("Connected to server.");

                break;
            }
            catch (HttpRequestException ex)
            {
                _baseLogger.LogWarning("Failed to connect to server.  Status Code: {code}", ex.StatusCode);
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "Error in hub connection.");
            }
            await Task.Delay(3_000, _cancellationToken);
        }
    }
    private class RetryPolicy : IRetryPolicy
    {
        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return TimeSpan.FromSeconds(3);
        }
    }
}
