using Medior.Reactive;
using Medior.Shared.Entities;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IApiService
    {
        Task<Result<UploadedFile>> UploadFile(byte[] fileBytes, string fileName);
        Task<Result<UploadedFile>> UploadFile(Stream fileStream, string fileName);
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

        public Task<Result<UploadedFile>> UploadFile(byte[] fileBytes, string fileName)
        {
            return UploadFile(new MemoryStream(fileBytes), fileName);
        }

        public async Task<Result<UploadedFile>> UploadFile(Stream fileStream, string fileName)
        {
            try
            {
                using var client = _clientFactory.CreateClient();
                var serverUri = _settings.ServerUri;

                var multiContent = new MultipartFormDataContent();
                var byteContent = new StreamContent(fileStream);
                multiContent.Add(byteContent, "file", fileName);

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
