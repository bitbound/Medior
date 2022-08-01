﻿using Medior.Shared;
using Medior.Shared.Entities;
using Medior.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Services
{
    public interface IApiService
    {
        Task<Result<UploadedFile>> UploadFile(byte[] fileBytes, string fileName);
        Task<Result<UploadedFile>> UploadFile(Stream fileStream, string fileName);
        Task<HttpResponseMessage> GetFileHeaders(string fileId);
    }
    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IServerUriProvider _uriProvider;
        private readonly ILogger<ApiService> _logger;

        public ApiService(
            IHttpClientFactory clientFactory,
            IServerUriProvider uriProvider,
            ILogger<ApiService> logger)
        {
            _clientFactory = clientFactory;
            _uriProvider = uriProvider;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> GetFileHeaders(string fileId)
        {
            using var client = _clientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Head, $"{_uriProvider.ServerUri}/api/file/{fileId}");
            return await client.SendAsync(request);
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
                var serverUri = _uriProvider.ServerUri;

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