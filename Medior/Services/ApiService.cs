using Medior.Shared.Entities;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IApiService
    {
        Task<Result<UploadedFile>> UploadFile(byte[] fileBytes);
    }
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ISettings _settings;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            IHttpClientFactory clientFactory, 
            ISettings settings,
            ILogger<ApiService> logger)
        {
            _clientFactory = clientFactory;
            _settings = settings;
            _logger = logger;
        }

        public async Task<Result<UploadedFile>> UploadFile(byte[] fileBytes)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var serverUri = _settings.ServerUri;

                var multiContent = new MultipartFormDataContent();
                var byteContent = new ByteArrayContent(fileBytes);
                multiContent.Add(byteContent, "file", "Medior_Upload.jpg");

                var response = await client.PostAsync($"{serverUri}/api/file", multiContent);
                response.EnsureSuccessStatusCode();

                var uploadedFile = await response.Content.ReadFromJsonAsync<UploadedFile>();
                if (uploadedFile is null)
                {
                    return Result.Fail<UploadedFile>("Response was empty.");
                }
                return Result.Ok(uploadedFile);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while uploading file.");
                return Result.Fail<UploadedFile>(ex);
            }
        }
    }
}
