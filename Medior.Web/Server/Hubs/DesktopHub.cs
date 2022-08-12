using Medior.Shared.Dtos;
using Medior.Shared.Interfaces;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace Medior.Web.Server.Hubs
{
    public class DesktopHub : Hub<IDesktopHubClient>
    {
        private readonly IClipboardSyncService _clipboardSync;

        public DesktopHub(IClipboardSyncService clipboardSyncService)
        {
            _clipboardSync = clipboardSyncService;
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public string GetClipboardReceiptToken()
        {
            return _clipboardSync.RegisterReceiver(Context.ConnectionId);
        }
    }
}
