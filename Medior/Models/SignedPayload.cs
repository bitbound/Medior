using System;

namespace Medior.Models
{
    public class SignedPayload
    {
        public byte[] Payload { get; init; } = Array.Empty<byte>();
        public byte[] Signature { get; init; } = Array.Empty<byte>();
    }
}
