using Medior.Shared.Entities;

namespace Medior.Web.Server.Models
{
    public class DesktopHubSession
    {
        public string ConnectionId { get; init; } = string.Empty;
        public UserAccount? User { get; set; }
        public string DeviceName { get; set; } = string.Empty;
    }
}
