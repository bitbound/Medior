using Medior.Shared.Auth;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Interfaces;
using Medior.Shared.Services;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Helpers
{
    public static class HttpHelper
    {
        internal static void ConfigureAuthenticatedClient(IServiceProvider services, HttpClient config)
        {
            var serverUri = services.GetRequiredService<IServerUriProvider>();
            var settings = services.GetRequiredService<ISettings>();
            var encryption = services.GetRequiredService<IEncryptionService>();
            var userAccount = new UserAccount()
            {
                PublicKey = settings.PublicKey,
                Username = settings.Username
            };
            var payload = MessagePackSerializer.Serialize(userAccount);
            var signature = encryption.Sign(payload);
            var dto = new SignedPayloadDto()
            {
                DtoType = DtoType.UserAccount,
                Payload = payload,
                Signature = signature
            };

            var dtoBytes = MessagePackSerializer.Serialize(dto);
            var base64Payload = Convert.ToBase64String(dtoBytes);
            config.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthSchemes.DigitalSignature, base64Payload);
            config.BaseAddress = new Uri(serverUri.ServerUri);
        }

        internal static void UpdateClientAuthorization(HttpClient config, UserAccount account, IEncryptionService encryption)
        {
            var payload = MessagePackSerializer.Serialize(account);
            var signature = encryption.Sign(payload);
            var dto = new SignedPayloadDto()
            {
                DtoType = DtoType.UserAccount,
                Payload = payload,
                Signature = signature
            };

            var dtoBytes = MessagePackSerializer.Serialize(dto);
            var base64Payload = Convert.ToBase64String(dtoBytes);
            config.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AuthSchemes.DigitalSignature, base64Payload);
        }
    }
}
