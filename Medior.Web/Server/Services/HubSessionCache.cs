using Medior.Web.Server.Models;
using System.Collections.Concurrent;

namespace Medior.Web.Server.Services;

public interface IHubSessionCache
{
    void AddDesktopSession(string connectionId, DesktopHubSession session);
    bool RemoveDesktopSession(string connectionId, out DesktopHubSession session);
}

public class HubSessionCache : IHubSessionCache
{
    private static readonly ConcurrentDictionary<string, DesktopHubSession> _desktopSessions = new();

    public void AddDesktopSession(string connectionId, DesktopHubSession session)
    {
        _desktopSessions.AddOrUpdate(connectionId, session, (k, v) => session);
    }

    public bool RemoveDesktopSession(string connectionId, out DesktopHubSession session)
    {
        if (_desktopSessions.TryRemove(connectionId, out session!))
        {
            return true;
        }
        session = DesktopHubSession.Empty;
        return false;
    }
}
