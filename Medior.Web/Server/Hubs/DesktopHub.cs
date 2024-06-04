using Medior.Shared.Interfaces;
using Medior.Shared.Models;
using Medior.Web.Server.Data;
using Medior.Web.Server.Models;
using Medior.Web.Server.Services;
using Microsoft.AspNetCore.SignalR;

namespace Medior.Web.Server.Hubs;

public class DesktopHub : Hub<IDesktopHubClient>
{
    private readonly AppDb _appDb;
    private readonly IHubSessionCache _hubSessions;
    private readonly IDesktopStreamCache _streamCache;
    private readonly IClipboardSyncService _clipboardSync;

    public DesktopHub(
        AppDb appDb,
        IHubSessionCache hubSessionCache,
        IDesktopStreamCache streamCache,
        IClipboardSyncService clipboardSyncService)
    {
        _appDb = appDb;
        _hubSessions = hubSessionCache;
        _streamCache = streamCache;
        _clipboardSync = clipboardSyncService;
    }

    public override async Task OnConnectedAsync()
    {
        var session = new DesktopHubSession()
        {
            ConnectionId = Context.ConnectionId
        };

        _hubSessions.AddDesktopSession(Context.ConnectionId, session);
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _hubSessions.RemoveDesktopSession(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }

    public string GetClipboardReceiptToken()
    {
        return _clipboardSync.RegisterReceiver(Context.ConnectionId);
    }

    public async Task SendStream(Guid streamId, IAsyncEnumerable<VideoChunk> stream)
    {
        var session = _streamCache.GetOrAdd(streamId, key => new StreamSignaler(streamId));

        try
        {
            session.Stream = stream;
            session.ReadySignal.Release();
            await session.EndSignal.WaitAsync(TimeSpan.FromHours(8));
        }
        finally
        {
            _streamCache.TryRemove(session.StreamId, out _);
        }
    }
}
