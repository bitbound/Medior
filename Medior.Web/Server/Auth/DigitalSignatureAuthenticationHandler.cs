using Medior.Shared.Auth;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Services;
using Medior.Web.Server.Data;
using MessagePack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Medior.Web.Server.Auth
{

    public class DigitalSignatureAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ILogger<DigitalSignatureAuthenticationHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        public DigitalSignatureAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory loggerFactory, 
            UrlEncoder encoder, 
            ISystemClock clock,
            IServiceScopeFactory scopeFactory,
            ILogger<DigitalSignatureAuthenticationHandler> logger) 
                : base(options, loggerFactory, encoder, clock)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            using var _ = _logger.BeginScope(nameof(HandleAuthenticateAsync));

            if (!CheckForAuthorizeAttribute())
            {
                return AuthenticateResult.NoResult();
            }

            var authHeader = Context.Request.Headers.Authorization.FirstOrDefault(x => 
                x.StartsWith(AuthSchemes.DigitalSignature));

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                return AuthenticateResult.Fail($"{AuthSchemes.DigitalSignature} authorization is missing.");
            }

            try
            {
                if (!TryGetSignedPayload(authHeader, out var signedDto))
                {
                    return AuthenticateResult.Fail("Failed to parse authorization header.");
                }

                return await VerifySignature(signedDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse header {authHeader}.", authHeader);
                return AuthenticateResult.Fail("An error occurred on the server.");
            }
        }

        private bool CheckForAuthorizeAttribute()
        {

            var metaData = Context.GetEndpoint()?.Metadata;

            if (metaData is null)
            {
                return false;
            }

            if (metaData.Any(x => x is HubMetadata))
            {
                var hubType = metaData.GetMetadata<HubMetadata>()?.HubType;
                if (hubType?.CustomAttributes.Any(x=>x.AttributeType == typeof(AuthorizeAttribute)) == true)
                {
                    return true;
                }
            }

            if (metaData.Any(x => x is ControllerActionDescriptor))
            {
                var descriptor = metaData.GetMetadata<ControllerActionDescriptor>();

                var controllerAuthorize = descriptor
                    ?.ControllerTypeInfo
                    ?.CustomAttributes
                    ?.Any(x => x.AttributeType == typeof(AuthorizeAttribute));

                var actionAuthorize = descriptor
                    ?.ControllerTypeInfo
                    ?.GetMethod(descriptor.ActionName)
                    ?.CustomAttributes
                    ?.Any(x => x.AttributeType == typeof(AuthorizeAttribute));

                if (controllerAuthorize == true || actionAuthorize == true)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryGetSignedPayload(string header, out SignedPayloadDto signedPayload)
        {
            var base64Token = header.Split(" ", 2).Skip(1).First();
            var payloadBytes = Convert.FromBase64String(base64Token);
            signedPayload = MessagePackSerializer.Deserialize<SignedPayloadDto>(payloadBytes);

            if (signedPayload.DtoType != Shared.Dtos.DtoType.UserAccount)
            {
                _logger.LogWarning("Unexpected DTO type of {type}.", signedPayload.DtoType);
                return false;
            }

            return true;
        }

        private async Task<AuthenticateResult> VerifySignature(SignedPayloadDto signedDto)
        {
            using var scope = _scopeFactory.CreateScope();
            using var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
            var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

            var account = MessagePackSerializer.Deserialize<UserAccount>(signedDto.Payload);
            var publicKey = account.PublicKey;

            var existingUser = await appDb.UserAccounts.FirstOrDefaultAsync(x => x.PublicKey == account.PublicKey);
            if (existingUser is not null)
            {
                publicKey = existingUser.PublicKey;
            }

            var publicBytes = Convert.FromBase64String(publicKey);
            encryption.ImportPublicKey(publicBytes);
            var result = encryption.Verify(signedDto.Payload, signedDto.Signature);

            if (!result)
            {
                return AuthenticateResult.Fail("Digital siganture verification failed.");
            }

            var claims = new Claim[]
            {
                new Claim(ClaimNames.PublicKey, publicKey),
                new Claim(ClaimNames.Username, account.Username),
                new Claim(ClaimNames.UserId, $"{existingUser?.Id}")
            };

            var identity = new ClaimsIdentity(claims, AuthSchemes.DigitalSignature);
            var principal = new ClaimsPrincipal(identity);
            return AuthenticateResult.Success(new AuthenticationTicket(principal, AuthSchemes.DigitalSignature));
        }
    }
}
