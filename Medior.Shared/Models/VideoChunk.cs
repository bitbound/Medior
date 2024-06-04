using MessagePack;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Medior.Shared.Models;

[DataContract]
public class VideoChunk
{
    [SerializationConstructor]
    [JsonConstructor]
    public VideoChunk(byte[] buffer, DateTimeOffset timestamp)
    {
        Buffer = buffer;
        Timestamp = timestamp;
    }

    [DataMember]
    public byte[] Buffer { get; set; }

    [DataMember]
    public DateTimeOffset Timestamp { get; set; }
}
