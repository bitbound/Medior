using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Medior.Shared.Models
{
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
}
