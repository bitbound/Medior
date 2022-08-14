using System;
using System.Runtime.Serialization;

namespace Medior.Shared.Dtos
{
    [DataContract]
    public class SignedPayloadDto
    {
        [DataMember]
        public byte[] Payload { get; init; } = Array.Empty<byte>();
        [DataMember]
        public byte[] Signature { get; init; } = Array.Empty<byte>();
        [DataMember]
        public DtoType DtoType { get; init; }
    }
}
