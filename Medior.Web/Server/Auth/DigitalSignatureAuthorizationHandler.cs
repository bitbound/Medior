using Medior.Shared.Auth;
using Medior.Shared.Dtos;
using Medior.Shared.Entities;
using Medior.Shared.Services;
using Medior.Web.Server.Data;
using MessagePack;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Medior.Web.Server.Auth
{
    public class DigitalSignatureAuthorizationHandler : AuthorizationHandler<DigitalSignatureRequirement>
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<DigitalSignatureAuthorizationHandler> _logger;

        public DigitalSignatureAuthorizationHandler(
            IServiceScopeFactory scopeFactory,
            IHttpContextAccessor contextAccessor,
            ILogger<DigitalSignatureAuthorizationHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DigitalSignatureRequirement requirement)
        {
            using var _ = _logger.BeginScope(nameof(HandleRequirementAsync));

            if (_contextAccessor.HttpContext is null)
            {
                context.Fail();
                return;
            }

            var authHeader = _contextAccessor.HttpContext.Request.Headers.Authorization.FirstOrDefault(x =>
                x.StartsWith(AuthSchemes.DigitalSignature));

            if (string.IsNullOrWhiteSpace(authHeader))
            {
                context.Fail(new AuthorizationFailureReason(this, $"{AuthSchemes.DigitalSignature} authorization is missing."));
                return;
            }

            try
            {
                if (!TryGetSignedPayload(authHeader, out var signedDto))
                {
                    context.Fail(new AuthorizationFailureReason(this, "Failed to parse authorization header."));
                    return;
                }

                var verifyResult = await VerifySignature(signedDto, context);
                if (!verifyResult)
                {
                    context.Fail(new(this, "Digital siganture verification failed."));
                    return;
                }

                context.Succeed(requirement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse header {authHeader}.", authHeader);
                context.Fail();
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

        private async Task<bool> VerifySignature(SignedPayloadDto signedDto, AuthorizationHandlerContext context)
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
