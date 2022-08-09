using Medior.Shared.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Medior.Web.Server.Hubs
{
    public class DesktopHub : Hub<IDesktopHubClient>
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
    }
}
