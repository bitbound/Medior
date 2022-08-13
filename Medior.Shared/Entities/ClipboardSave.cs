using Medior.Shared.Entities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Shared.Entities
{
    [DataContract]
    public class ClipboardSave
    {
        [DataMember]
        public string AccessTokenEdit { get; set; } = string.Empty;
        [DataMember]
        public string AccessTokenView { get; set; } = string.Empty;
        [DataMember]
        public byte[] Content { get; init; } = Array.Empty<byte>();
        [DataMember]
        public long ContentSize { get; init; }

        [DataMember]
        public ClipboardContentType ContentType { get; init; }

        [DataMember]
        public DateTimeOffset CreatedAt { get; init; }

        [DataMember]
        public string FriendlyName { get; set; } = string.Empty;

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        [DataMember]
        public bool IsEncrypted { get; init; }
        [DataMember]
        public DateTimeOffset LastAccessed { get; set; }
    }
}
