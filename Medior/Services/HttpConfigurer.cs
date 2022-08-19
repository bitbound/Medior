using Medior.Shared.Auth;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IHttpConfigurer
    {
        void ConfigureAuthenticatedClient(HttpClient client);
        HttpClient GetAuthorizedClient();
        string GetDigitalSignature();
        string GetDigitalSignature(UserAccount account);
        void UpdateClientAuthorizations(UserAccount account);
    }

    public class HttpConfigurer : IHttpConfigurer
    {
        public readonly IHttpClientFactory _clientFactory;
        private static readonly ConcurrentBag<HttpClient> _clients = new();
        private readonly IEncryptionService _encryption;
        private readonly IServerUriProvider _serverUri;
        private readonly ISettings _settings;
        public HttpConfigurer(
            IHttpClientFactory clientFactory,
            IServerUriProvider serverUri,
            ISettings settings,
            IEncryptionService encryption)
        {
            _clientFactory = clientFactory;
            _serverUri = serverUri;
            _settings = settings;
            _encryption = encryption;
        }

        public void ConfigureAuthenticatedClient(HttpClient client)
        {
            var userAccount = new UserAccount()
            {
                PublicKey = _settings.PublicKey,
                Username = _settings.Username
            };

            var signature = GetDigitalSignature(userAccount);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthSchemes.DigitalSignature, signature);

            client.BaseAddress = new Uri(_serverUri.ServerUri);
            _clients.Add(client);
        }

        public HttpClient GetAuthorizedClient()
        {
            var client = _clientFactory.CreateClient();
            ConfigureAuthenticatedClient(client);
            return client;
        }

        public string GetDigitalSignature(UserAccount account)
        {
            var payload = MessagePackSerializer.Serialize(account);
            var signature = _encryption.Sign(payload);
            var dto = new SignedPayloadDto()
            {
                DtoType = DtoType.UserAccount,
                Payload = payload,
                Signature = signature
            };

            var dtoBytes = MessagePackSerializer.Serialize(dto);
            var base64Payload = Convert.ToBase64String(dtoBytes);
            return base64Payload;
        }

        public string GetDigitalSignature()
        {
            return GetDigitalSignature(new UserAccount()
            {
                PublicKey = _settings.PublicKey,
                Username = _settings.Username
            });
        }

        public void UpdateClientAuthorizations(UserAccount account)
        {

            var signature = GetDigitalSignature(account);

            foreach (var client in _clients)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthSchemes.DigitalSignature, signature);
            }
        }
    }
}
