using Medior.Shared.Dtos;
using Medior.Shared.Interfaces;
using MessagePack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Services.Http
{
    public interface IClipboardApi
    {
        Task<Result<string>> SetClipboardContent(ClipboardContentDto content);
        Task<Result<ClipboardContentDto>> GetClipboardContent(string accessToken);
        Task<Result> SendToReceiver(string receiptToken, ClipboardContentDto content);
    }
    public class ClipboardApi : IClipboardApi
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IServerUriProvider _uri;
        private readonly ILogger<ClipboardApi> _logger;

        public ClipboardApi(
            IHttpClientFactory clientFactory,
            IServerUriProvider serverUri,
            ILogger<ClipboardApi> logger)
        {
            _clientFactory = clientFactory;
            _uri = serverUri;
            _logger = logger;
        }

        public async Task<Result<ClipboardContentDto>> GetClipboardContent(string accessToken)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var response = await client.GetByteArrayAsync($"{_uri.ServerUri}/api/clipboard/{accessToken}");
                var dto = MessagePackSerializer.Deserialize<ClipboardContentDto>(response);
                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting clipboard content.");
                return Result.Fail<ClipboardContentDto>(ex);
            }
        }


        public async Task<Result> SendToReceiver(string receiptToken, ClipboardContentDto content)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var serializedDto = MessagePackSerializer.Serialize(content);
                var httpContent = new ByteArrayContent(serializedDto);
                var response = await client.PostAsync($"{_uri.ServerUri}/api/clipboard/{receiptToken}", httpContent);
                response.EnsureSuccessStatusCode();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending clipboard content.");
                return Result.Fail(ex);
            }
        }

        public async Task<Result<string>> SetClipboardContent(ClipboardContentDto content)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var serializedDto = MessagePackSerializer.Serialize(content);
                var httpContent = new ByteArrayContent(serializedDto);
                var response = await client.PostAsync($"{_uri.ServerUri}/api/clipboard", httpContent);
                response.EnsureSuccessStatusCode();
                var accessToken = await response.Content.ReadAsStringAsync();
                var accessUrl = $"{_uri.ServerUri}/clipboard-sync?accessToken={accessToken}";
                return Result.Ok(accessUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while posting clipboard content.");
                return Result.Fail<string>(ex);
            }
        }
    }
}
