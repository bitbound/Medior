namespace Medior.Shared.Models;

public struct StreamToken : IEquatable<StreamToken>
{
    public StreamToken(Guid sessionId, Guid requestId)
    {
        SessionId = sessionId;
        RequestId = requestId;
    }

    public static StreamToken Empty { get; } = new();

    public Guid SessionId { get; }
    public Guid RequestId { get; }

    public bool Equals(StreamToken other)
    {
        return
            SessionId == other.SessionId &&
            RequestId == other.RequestId;
    }

    public override bool Equals(object? obj)
    {
        return obj is StreamToken other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SessionId, RequestId);
    }

    public static bool operator ==(StreamToken left, StreamToken right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(StreamToken left, StreamToken right)
    {
        return !(left == right);
    }
}
