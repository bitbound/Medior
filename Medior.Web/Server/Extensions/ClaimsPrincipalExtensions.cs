using Medior.Web.Server.Auth;
using System.Security.Claims;

namespace Medior.Web.Server.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasAccountOnServer(this ClaimsPrincipal user)
        {
            return user.HasClaim(x => x.Type == ClaimNames.UserId && !string.IsNullOrWhiteSpace(x.Value));
        }

        public static bool TryGetClaim(this ClaimsPrincipal user, string claimType, out string claimValue)
        {
            claimValue = user.Claims.FirstOrDefault(x => x.Type == claimType)?.Value ?? string.Empty;
            return !string.IsNullOrWhiteSpace(claimValue);
        }

        public static bool TryGetUserId(this ClaimsPrincipal user, out Guid userId)
        {
            userId = Guid.Empty;

            if (!TryGetClaim(user, ClaimNames.UserId, out var claimId))
            {
                return false;
            }

            if (!Guid.TryParse(claimId, out userId))
            {
                return false;
            }

            return true;
        }
    }
}
