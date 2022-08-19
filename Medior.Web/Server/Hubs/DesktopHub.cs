using Medior.Shared.Dtos;
using Medior.Shared.Interfaces;
using Medior.Web.Server.Data;
using Medior.Web.Server.Extensions;
using Medior.Web.Server.Models;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Medior.Web.Server.Hubs
{
    public class DesktopHub : Hub<IDesktopHubClient>
    {
        private readonly AppDb _appDb;
        private readonly IClipboardSyncService _clipboardSync;
        private readonly ConcurrentDictionary<string, DesktopHubSession> _sessions = new();

        public DesktopHub(
            AppDb appDb,
            IClipboardSyncService clipboardSyncService)
        {
            _appDb = appDb;
            _clipboardSync = clipboardSyncService;
        }

        public override async Task OnConnectedAsync()
        {
            var session = new DesktopHubSession()
            {
                ConnectionId = Context.ConnectionId
            };

            if (Context.User?.TryGetUserId(out var userId) == true)
            {
                var user = await _appDb.UserAccounts.FindAsync(userId);
                session.User = user;
            }

            _sessions.AddOrUpdate(Context.ConnectionId, session, (k, v) => session);

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _sessions.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        public string GetClipboardReceiptToken()
        {
            return _clipboardSync.RegisterReceiver(Context.ConnectionId);
        }
    }
}
