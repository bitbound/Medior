using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Models
{
    public class SignedPayload
    {
        public byte[] Payload { get; init; } = Array.Empty<byte>();
        public byte[] Signature { get; init; } = Array.Empty<byte>();
    }
}
