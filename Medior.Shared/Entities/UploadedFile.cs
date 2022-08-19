using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Medior.Shared.Entities
{
    [DataContract]
    public class UploadedFile
    {
        [DataMember]
        public string AccessTokenEdit { get; set; } = string.Empty;

        [DataMember]
        public string AccessTokenView { get; set; } = string.Empty;

        [DataMember]
        public string ContentDisposition { get; init; } = string.Empty;

        [DataMember]
        public string FileName { get; init; } = string.Empty;

        [DataMember]
        public long FileSize { get; init; }

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }


        [DataMember]
        public DateTimeOffset LastAccessed { get; set; }

        [DataMember]
        public DateTimeOffset UploadedAt { get; init; }

        [DataMember]
        public UserAccount? UserAccount { get; init; }

        [DataMember]
        public Guid? UserAccountId { get; init; }
    }
}
