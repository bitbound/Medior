using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Medior.Shared.Dtos;
using Medior.Shared.Helpers;
using Medior.Shared.Interfaces;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medior.Shared.SignalR
{
    public abstract class HubConnectionBase
    {
        private static readonly ConcurrentDictionary<Guid, byte[]> _dtoChunks = new();
        private readonly ILogger<HubConnectionBase> _baseLogger;
        private readonly IHubConnectionBuilder _builder;
        private readonly IDtoHandler _dtoHandler;
        private HubConnection? _connection;

        public HubConnectionBase(
            IHubConnectionBuilder builder,
            IDtoHandler dtoHandler,
            ILogger<HubConnectionBase> logger)
        {
            _builder = builder;
            _dtoHandler = dtoHandler;
            _baseLogger = logger;
        }

        protected HubConnection? Connection => _connection;

        public async Task Connect(string hubUrl, Action<HubConnection> configure, CancellationToken cancellationToken)
        {
            _connection = _builder
                .WithUrl(hubUrl)
                .AddMessagePackProtocol()
                .WithAutomaticReconnect(new RetryPolicy())
                .Build();

            _connection.On<DtoWrapper>("ReceiveDto", ReceiveDto);
            _connection.Reconnecting += HubConnection_Reconnecting;
            _connection.Reconnected += HubConnection_Reconnected;

            configure.Invoke(_connection);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _baseLogger.LogInformation("Connecting to server.");

                    await _connection.StartAsync(cancellationToken);

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
                await Task.Delay(3_000, cancellationToken);
            }
        }

        protected async Task SendDtoToPeer<T>(T dto, DtoType dtoType, Guid requestId, HubType peerHubType, string requesterConnectionId)
        {
            foreach (var wrapper in DtoChunker.ChunkDto(dto, dtoType, requestId))
            {
                await _connection!.InvokeAsync("SendDtoToPeer", wrapper, peerHubType, requesterConnectionId);
            }
        }

        private async Task ReceiveDto(DtoWrapper dto)
        {
            await _dtoHandler.ReceiveDto(dto);
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

        private class RetryPolicy : IRetryPolicy
        {
            public TimeSpan? NextRetryDelay(RetryContext retryContext)
            {
                return TimeSpan.FromSeconds(3);
            }
        }
    }
}
