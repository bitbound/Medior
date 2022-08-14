using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Interfaces;
using MessagePack;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Services.Http
{
    public interface IClipboardApi
    {
        Task<Result> DeleteClip(ClipboardSaveDto clip);

        Task<Result<ClipboardContentDto>> GetClipboardContent(string clipId, string accessToken);

        Task<Result<ClipboardSaveDto>> SaveClipboardContent(ClipboardContentDto content);
        Task<Result> SendToReceiver(string receiptToken, ClipboardContentDto content);
        Task<Result> UpdateClip(ClipboardSaveDto dto);
    }
    public class ClipboardApi : IClipboardApi
    {
        private readonly HttpClient _client;
        private readonly ILogger<ClipboardApi> _logger;
        private readonly IServerUriProvider _uri;
        public ClipboardApi(
            HttpClient client,
            IServerUriProvider serverUri,
            ILogger<ClipboardApi> logger)
        {
            _client = client;
            _uri = serverUri;
            _logger = logger;
        }

        public async Task<Result> DeleteClip(ClipboardSaveDto clip)
        {
            try
            {
                var response = await _client.DeleteAsync($"{_uri.ServerUri}/api/clipboard/{clip.Id}?accessToken={clip.AccessTokenEdit}");
                response.EnsureSuccessStatusCode();
                return Result.Ok();
            }
            catch (HttpRequestException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting clip.");
                return Result.Fail(ex);
            }
        }

        public async Task<Result<ClipboardContentDto>> GetClipboardContent(string clipId, string accessToken)
        {
            try
            {
                var dto = await _client.GetFromJsonAsync<ClipboardContentDto>($"{_uri.ServerUri}/api/clipboard/{clipId}/{accessToken}");
                if (dto is null)
                {
                    return Result.Fail<ClipboardContentDto>("Response was empty.");
                }
                return Result.Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting clipboard content.");
                return Result.Fail<ClipboardContentDto>(ex);
            }
        }


        public async Task<Result<ClipboardSaveDto>> SaveClipboardContent(ClipboardContentDto content)
        {
            try
            {
                var response = await _client.PostAsJsonAsync($"{_uri.ServerUri}/api/clipboard", content);
                response.EnsureSuccessStatusCode();
                var clipSave = await response.Content.ReadFromJsonAsync<ClipboardSaveDto>();
                if (clipSave is null)
                {
                    return Result.Fail<ClipboardSaveDto>("Response was empty.");
                }
                return Result.Ok(clipSave);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while posting clipboard content.");
                return Result.Fail<ClipboardSaveDto>(ex);
            }
        }

        public async Task<Result> SendToReceiver(string receiptToken, ClipboardContentDto content)
        {
            try
            {
                var response = await _client.PostAsJsonAsync($"{_uri.ServerUri}/api/clipboard/{receiptToken}", content);
                response.EnsureSuccessStatusCode();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending clipboard content.");
                return Result.Fail(ex);
            }
        }
        public async Task<Result> UpdateClip(ClipboardSaveDto dto)
        {
            try
            {
                var response = await _client.PutAsJsonAsync($"{_uri.ServerUri}/api/clipboard", dto);
                response.EnsureSuccessStatusCode();
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating clipboard content.");
                return Result.Fail(ex);
            }
        }
    }
}
