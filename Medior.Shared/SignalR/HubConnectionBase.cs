using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medior.Shared.SignalR
{
    public abstract class HubConnectionBase
    {
        private readonly IHubConnectionBuilder _builder;
        private readonly ILogger<HubConnectionBase> _baseLogger;
        private HubConnection? _connection;

        public HubConnectionBase(IHubConnectionBuilder builder, ILogger<HubConnectionBase> logger)
        {
            _builder = builder;
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
