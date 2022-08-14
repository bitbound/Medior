using Medior.Shared.Auth;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Services;
using Medior.Web.Server.Data;
using MessagePack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Medior.Web.Server.Auth
{
    public class DigitalSignatureFilterAttribute : IAsyncAuthorizationFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DigitalSignatureAuthorizationHandler> _logger;

        public DigitalSignatureFilterAttribute(
            IServiceScopeFactory scopeFactory,
            ILogger<DigitalSignatureAuthorizationHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            using var _ = _logger.BeginScope(nameof(OnAuthorizationAsync));

            var authHeader = context.HttpContext.Request.Headers.Authorization.FirstOrDefault(x =>
                x.StartsWith(AuthSchemes.DigitalSignature));

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                if (!TryGetSignedPayload(authHeader, out var signedDto))
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }

                var verifyResult = await VerifySignature(signedDto);
                if (!verifyResult)
                {
                    context.Result = new UnauthorizedResult();
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse header {authHeader}.", authHeader);
                context.Result = new UnauthorizedResult();
            }
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

        private async Task<bool> VerifySignature(SignedPayloadDto signedDto)
        {
            using var scope = _scopeFactory.CreateScope();
            using var appDb = scope.ServiceProvider.GetRequiredService<AppDb>();
            var encryption = scope.ServiceProvider.GetRequiredService<IEncryptionService>();

            var account = MessagePackSerializer.Deserialize<UserAccount>(signedDto.Payload);
            var publicKey = account.PublicKey;

            var existingUser = await appDb.UserAccounts.FirstOrDefaultAsync(x => x.Username == account.Username);
            if (existingUser is not null)
            {
                publicKey = existingUser.PublicKey;
            }

            var publicBytes = Convert.FromBase64String(publicKey);
            encryption.ImportPublicKey(publicBytes);
            var result = encryption.Verify(signedDto.Payload, signedDto.Signature);

            if (!result)
            {
                return false;
            }

            var claims = new Claim[]
              {
                        new Claim("PublicKey", publicKey),
                        new Claim("Username", account.Username)
              };
            var identity = new ClaimsIdentity(claims, AuthSchemes.DigitalSignature);
            var principal = new ClaimsPrincipal(identity);
            return true;
        }
    }
}
