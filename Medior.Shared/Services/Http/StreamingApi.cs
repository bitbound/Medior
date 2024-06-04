using Medior.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace Medior.Shared.Services.Http;

public interface IStreamingApi
{
    Task<Result> GetStream(Guid streamId, Stream inputStream, CancellationToken cancellationToken);
}

public class StreamingApi : IStreamingApi
{
    private readonly HttpClient _client;
    private readonly ILogger<StreamingApi> _logger;
    private readonly IServerUriProvider _uri;

    public StreamingApi(
        HttpClient client,
        IServerUriProvider uriProvider,
        ILogger<StreamingApi> logger)
    {
        _client = client;
        _uri = uriProvider;
        _logger = logger;
    }

    public async Task<Result> GetStream(Guid streamId, Stream inputStream, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _client.GetAsync($"{_uri.ServerUri}/api/streaming/{streamId}", cancellationToken);
            response.EnsureSuccessStatusCode();
            await response.Content.CopyToAsync(inputStream);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while sending stream.");
            return Result.Fail(ex);
        }
    }
}
