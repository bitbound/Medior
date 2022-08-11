using Medior.Shared.Dtos.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Dtos
{
    [DataContract]
    public class ClipboardContentDto
    {
        [DataMember]
        public byte[] Content { get; init; } = Array.Empty<byte>();

        [DataMember]
        public ClipboardContentType Type { get; init; }
    }
}
